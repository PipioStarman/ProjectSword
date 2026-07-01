using UnityEngine;

public class Scaler : MonoBehaviour
{
    private enum TipoRotacionEscalada
    {
        Arriba,
        Adelante,
        Izquierda,
        Derecha
    }

    private TipoRotacionEscalada tipoRotacionEscalada =
        TipoRotacionEscalada.Arriba;

    // ============================================================
    // ROTACIÓN DURANTE EL ESCALADO
    // ============================================================
    [Header("Retorno por Doble Clic")]
    [Tooltip("Tiempo máximo en segundos entre pulsaciones para detectar doble clic.")]
    public float tiempoDobleClic = 0.3f;
    private float ultimoTiempoPulsadoF = -999f;

    [Header("Rotación mientras escalas")]
    public float gradosRotacionEscalada = 90f;

    public float bufferSoltarTeclaAntesDeF = 0.08f;

    private float tiempoDesdeSoltarTeclaRotacion = 999f;

    private bool wEstabaPulsada;
    private bool aEstabaPulsada;
    private bool dEstabaPulsada;

    private Quaternion rotacionExtraEscalada = Quaternion.identity;
    private bool hayRotacionExtraEscalada;

    // ============================================================
    // COMPONENTES
    // ============================================================

    [Header("Configuración de Componentes")]
    public Rigidbody rb;
    public BoxCollider coll;
    public Material matEspada;

    // ============================================================
    // ESCALADO
    // ============================================================

    [Header("Ajustes de Escalado")]
    public float velocidadEscala = 2500f;
    public float escalaMinima = 0.1f;

    private Color colorOriginal;
    private bool estaTocando;
    public bool estaEnModoFantasma = false;

    public Transform SwordLocationReference;
    public GameObject FakeSword;

    // ============================================================
    // FANTASMA DEL SUELO
    // ============================================================

    [Header("Prototipo Fantasma en Suelo")]
    public Transform SueloLocationReference;
    public GameObject SwordFloorGhost;

    // ============================================================
    // PERSONAJE
    // ============================================================

    [Header("Conexión con el Personaje")]
    public PhysicsCharacterController.InputReader inputReader;

    // ============================================================
    // CAÍDA DESDE UN EXTREMO
    // ============================================================

    [Header("Caída desde un extremo")]

    [Tooltip("Punto de la espada que queda fijado temporalmente.")]
    public Transform puntoPivoteCaida;

    [Header("Duración de anclaje según rotación")]
    public float duracionAnclajeNormal = 0.35f;
    public float duracionAnclajeHaciaAdelante = 0.15f;
    public float duracionAnclajeLateral = 0.35f;

    [Header("Velocidad de caída según orientación")]
    public float velocidadCaidaNormal = 1.5f;
    public float velocidadCaidaHaciaAdelante = 0.8f;
    public float velocidadCaidaLateral = 1.5f;

    // ============================================================
    // NUEVO: REFUERZO DE LA VELOCIDAD INICIAL
    // ============================================================

    [Header("Estabilidad del empuje")]

    [Tooltip(
        "Número de pasos físicos durante los que se reafirma " +
        "la velocidad inicial. Prueba con 2."
    )]
    [Range(1, 4)]
    public int pasosRefuerzoVelocidad = 2;

    private ConfigurableJoint jointTemporal;
    private float tiempoAnclaje;

    // ============================================================
    // NUEVO: CAÍDA EN DOS PASOS DE FÍSICA
    // ============================================================

    private bool crearJointPendiente;
    private bool aplicarVelocidadPendiente;

    private Vector3 ejeRotacionPendiente;
    private float velocidadCaidaPendiente;
    private float duracionAnclajePendiente;

    private int pasosRefuerzoRestantes;

    // ================================
    // NUEVO: Escalado con ratón
    // ================================

    [SerializeField] private float sensibilidadRatonX = 2f;
    [SerializeField] private float sensibilidadRatonY = 2f;

    // ============================================================
    // INICIALIZACIÓN
    // ============================================================

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (coll == null)
            coll = GetComponent<BoxCollider>();

        if (matEspada != null)
        {
            matEspada.color = Color.green;
            colorOriginal = matEspada.color;
        }

        if (SwordFloorGhost != null)
            SwordFloorGhost.SetActive(false);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // DETECCIÓN DE DOBLE CLIC EN "F"
    if (Input.GetKeyDown(KeyCode.LeftControl))
    {
        RetornarEspadaAlPersonaje();
    }
        ActualizarRotacionPorTeclas();

        bool botonPulsado =
            inputReader != null &&
            inputReader.scale;

        if (botonPulsado || estaTocando)
        {
            EntrarEnModoFantasma();
            ProcesarEscalado();

            if (inputReader != null)
                inputReader.scale = true;
        }
        else
        {
            SalirDeModoFantasma();
        }
    }
    private void RetornarEspadaAlPersonaje()
    {

        Debug.Log("DEVUELVE ESPADA");

        FakeSword.SetActive(true);
        
    }
    // ============================================================
    // ELEGIR DURACIÓN DEL ANCLAJE
    // ============================================================

    private float ObtenerDuracionAnclaje()
    {
        switch (tipoRotacionEscalada)
        {
            case TipoRotacionEscalada.Adelante:
                return duracionAnclajeHaciaAdelante;

            case TipoRotacionEscalada.Izquierda:
            case TipoRotacionEscalada.Derecha:
                return duracionAnclajeLateral;

            default:
                return duracionAnclajeNormal;
        }
    }

    // ============================================================
    // ELEGIR VELOCIDAD DE CAÍDA
    // ============================================================

    private float ObtenerVelocidadCaida()
    {
        switch (tipoRotacionEscalada)
        {
            case TipoRotacionEscalada.Arriba:
                return velocidadCaidaNormal;

            case TipoRotacionEscalada.Adelante:
                return velocidadCaidaHaciaAdelante;

            case TipoRotacionEscalada.Izquierda:
            case TipoRotacionEscalada.Derecha:
                return velocidadCaidaLateral;

            default:
                
                return velocidadCaidaNormal;
        }
    }

    // ============================================================
    // MODIFICADO: FÍSICA DE CAÍDA EN DOS PASOS
    // ============================================================

    private void FixedUpdate()
    {
        // ========================================================
        // PASO 1: CREAR EL JOINT
        // ========================================================
        if (crearJointPendiente)
        {
            crearJointPendiente = false;

            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            CrearPivoteTemporal(duracionAnclajePendiente);

            aplicarVelocidadPendiente = true;
            return; // Esperamos al siguiente paso físico para inyectar la fuerza
        }

        // ========================================================
        // PASO 2: INYECTAR VELOCIDAD Y ASIGNAR TIEMPO REAL
        // ========================================================
        if (aplicarVelocidadPendiente)
        {
            aplicarVelocidadPendiente = false;

            rb.WakeUp();
            pasosRefuerzoRestantes = Mathf.Max(1, pasosRefuerzoVelocidad);

            // El tiempo empieza a contar AQUÍ, cuando la fuerza se aplica
            tiempoAnclaje = duracionAnclajePendiente;
        }

        // ========================================================
        // REFUERZO DE VELOCIDAD (Evita que PhysX absorba el empuje)
        // ========================================================
        if (pasosRefuerzoRestantes > 0)
        {
            rb.WakeUp();
            // Aplicamos fuerza bruta al Rigidbody para vencer la inercia del cambio cinemático
            rb.angularVelocity = ejeRotacionPendiente * velocidadCaidaPendiente;
            pasosRefuerzoRestantes--;
        }

        // ========================================================
        // GESTIÓN DEL TIEMPO DEL ANCLAJE
        // ========================================================
        // Solo restamos tiempo si el joint existe y ya se ha iniciado la caída
        if (jointTemporal != null && !aplicarVelocidadPendiente)
        {
            tiempoAnclaje -= Time.fixedDeltaTime;

            if (tiempoAnclaje <= 0f)
            {
                EliminarPivoteTemporal();
            }
        }
    }

    // ============================================================
    // ROTACIÓN CON W / A / D
    // ============================================================

    private void ActualizarRotacionPorTeclas()
    {
        bool wPulsadaAhora = Input.GetKey(KeyCode.W);
        bool aPulsadaAhora = Input.GetKey(KeyCode.A);
        bool dPulsadaAhora = Input.GetKey(KeyCode.D);

        bool algunaTeclaRotacionPulsada =
            wPulsadaAhora ||
            aPulsadaAhora ||
            dPulsadaAhora;

        bool algunaTeclaEstabaPulsada =
            wEstabaPulsada ||
            aEstabaPulsada ||
            dEstabaPulsada;

        // Prioridad: W > D > A.

        if (wPulsadaAhora)
        {
            rotacionExtraEscalada =
                Quaternion.Euler(
                    0f,
                    gradosRotacionEscalada,
                    0f
                );

            tipoRotacionEscalada =
                TipoRotacionEscalada.Adelante;

            hayRotacionExtraEscalada = true;
            tiempoDesdeSoltarTeclaRotacion = 0f;
        }
        else if (dPulsadaAhora)
        {
            rotacionExtraEscalada =
                Quaternion.Euler(
                    0f,
                    0f,
                    -gradosRotacionEscalada
                );

            tipoRotacionEscalada =
                TipoRotacionEscalada.Derecha;

            hayRotacionExtraEscalada = true;
            tiempoDesdeSoltarTeclaRotacion = 0f;
        }
        else if (aPulsadaAhora)
        {
            rotacionExtraEscalada =
                Quaternion.Euler(
                    0f,
                    0f,
                    gradosRotacionEscalada
                );

            tipoRotacionEscalada =
                TipoRotacionEscalada.Izquierda;

            hayRotacionExtraEscalada = true;
            tiempoDesdeSoltarTeclaRotacion = 0f;
        }

        if (
            algunaTeclaEstabaPulsada &&
            !algunaTeclaRotacionPulsada
        )
        {
            tiempoDesdeSoltarTeclaRotacion = 0f;
        }
        else if (!algunaTeclaRotacionPulsada)
        {
            tiempoDesdeSoltarTeclaRotacion += Time.deltaTime;
        }

        wEstabaPulsada = wPulsadaAhora;
        aEstabaPulsada = aPulsadaAhora;
        dEstabaPulsada = dPulsadaAhora;
    }

    private bool DebeMantenerRotacionExtra()
    {
        bool teclaRotacionPulsadaAhora =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D);

        bool teclaRotacionSoltadaHaceMuyPoco =
            tiempoDesdeSoltarTeclaRotacion <=
            bufferSoltarTeclaAntesDeF;

        return hayRotacionExtraEscalada &&
               (
                   teclaRotacionPulsadaAhora ||
                   teclaRotacionSoltadaHaceMuyPoco
               );
    }

    // ============================================================
    // MODO FANTASMA
    // ============================================================

    private void EntrarEnModoFantasma()
    {
        if (!estaEnModoFantasma)
        {
            CancelarCaidaPendiente();
            EliminarPivoteTemporal();
        }

        estaEnModoFantasma = true;

        transform.position =
            SwordLocationReference.position;

        if (DebeMantenerRotacionExtra())
        {
            transform.rotation =
                SwordLocationReference.rotation *
                rotacionExtraEscalada;
        }
        else
        {
            transform.rotation =
                SwordLocationReference.rotation;
        }

        if (FakeSword != null)
            FakeSword.SetActive(false);

        rb.isKinematic = true;
        coll.isTrigger = true;
        estaEnModoFantasma = true; // ← añadir

        if (
            SwordFloorGhost != null &&
            SueloLocationReference != null
        )
        {
            SwordFloorGhost.SetActive(true);

            SwordFloorGhost.transform.position =
                SueloLocationReference.position;

            SwordFloorGhost.transform.rotation =
                transform.rotation;

            SwordFloorGhost.transform.localScale =
                transform.localScale;
        }

        if (matEspada != null)
        {
            if (estaTocando)
            {
                matEspada.color = Color.red;
            }
            else
            {
                Color colorFantasma = colorOriginal;
                colorFantasma.a = 0.5f;

                matEspada.color = colorFantasma;
            }
        }
    }

    private void SalirDeModoFantasma()
    {
        if (estaTocando) return;
        if (!estaEnModoFantasma) return;

        estaEnModoFantasma = false;

        if (!rb.isKinematic)
            return;

        bool habiaRotacionExtra = DebeMantenerRotacionExtra();

        bool rotadaLateralmente = habiaRotacionExtra &&
            (tipoRotacionEscalada == TipoRotacionEscalada.Izquierda ||
             tipoRotacionEscalada == TipoRotacionEscalada.Derecha);

        bool rotadaHaciaAdelante = habiaRotacionExtra &&
            tipoRotacionEscalada == TipoRotacionEscalada.Adelante;

        // Aplicar rotación final antes de activar físicas
        if (habiaRotacionExtra)
        {
            transform.rotation = SwordLocationReference.rotation * rotacionExtraEscalada;
        }
        else
        {
            transform.rotation = SwordLocationReference.rotation;
        }

        rb.isKinematic = false;
        coll.isTrigger = false;

        // Sincronización imperativa para Unity 6 / PhysX 4
        Physics.SyncTransforms();
        rb.WakeUp();

        if (matEspada != null)
            matEspada.color = colorOriginal;

        if (SwordFloorGhost != null)
            SwordFloorGhost.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // DETERMINAR DIRECCIÓN USANDO SIEMPRE LA REFERENCIA FLUIDA
        Vector3 direccionEmpuje = SwordLocationReference.right;
        direccionEmpuje.y = 0f;

        if (direccionEmpuje.sqrMagnitude < 0.001f)
        {
            direccionEmpuje = SwordLocationReference.forward;
            direccionEmpuje.y = 0f;
        }
        direccionEmpuje.Normalize();

        // Guardamos los datos para el FixedUpdate antes de limpiar las variables de control
        PrepararCaidaDesdeExtremo(direccionEmpuje);

        // RESET DE CONTROL DE INPUTS
        hayRotacionExtraEscalada = false;
        rotacionExtraEscalada = Quaternion.identity;
        tipoRotacionEscalada = TipoRotacionEscalada.Arriba;
        tiempoDesdeSoltarTeclaRotacion = 999f;
    }

    // ============================================================
    // MODIFICADO: PREPARAR CAÍDA PARA LOS SIGUIENTES FIXED UPDATE
    // ============================================================

    private void PrepararCaidaDesdeExtremo(Vector3 direccionEmpuje)
    {
        if (puntoPivoteCaida == null)
        {
            Debug.LogWarning("Falta asignar puntoPivoteCaida.");

            return;
        }

        Vector3 ejeRotacion = Vector3.Cross(Vector3.up,direccionEmpuje);

        if (ejeRotacion.sqrMagnitude < 0.001f)
        {
            ejeRotacion =
                SwordLocationReference.forward;
        }

        ejeRotacion.Normalize();

        // ========================================================
        // NUEVO:
        // Guardamos todos los valores antes de resetear
        // tipoRotacionEscalada.
        // ========================================================

        ejeRotacionPendiente =
            ejeRotacion;

        velocidadCaidaPendiente = ObtenerVelocidadCaida();

        duracionAnclajePendiente = ObtenerDuracionAnclaje();

        crearJointPendiente = true;
        aplicarVelocidadPendiente = false;
        pasosRefuerzoRestantes = 0;
    }

    // ============================================================
    // NUEVO: CANCELAR ESTADO DE CAÍDA
    // ============================================================

    private void CancelarCaidaPendiente()
    {
        crearJointPendiente = false;
        aplicarVelocidadPendiente = false;
        pasosRefuerzoRestantes = 0;
    }

    // ============================================================
    // MODIFICADO: JOINT TEMPORAL
    // ============================================================

    private void CrearPivoteTemporal(float duracion)
    {
        if (puntoPivoteCaida == null) return;

        EliminarPivoteTemporal();

        jointTemporal = gameObject.AddComponent<ConfigurableJoint>();
        jointTemporal.connectedBody = null;
        jointTemporal.autoConfigureConnectedAnchor = false;

        jointTemporal.anchor = transform.InverseTransformPoint(puntoPivoteCaida.position);
        jointTemporal.connectedAnchor = puntoPivoteCaida.position;

        jointTemporal.xMotion = ConfigurableJointMotion.Locked;
        jointTemporal.yMotion = ConfigurableJointMotion.Locked;
        jointTemporal.zMotion = ConfigurableJointMotion.Locked;

        jointTemporal.angularXMotion = ConfigurableJointMotion.Free;
        jointTemporal.angularYMotion = ConfigurableJointMotion.Free;
        jointTemporal.angularZMotion = ConfigurableJointMotion.Free;

        jointTemporal.enableCollision = false;

        // CONFIGURACIÓN DE ALTA PRECISIÓN (Evita elongaciones y pérdidas de fuerza)
        jointTemporal.projectionMode = JointProjectionMode.PositionAndRotation;
        jointTemporal.projectionDistance = 0.01f; // Mucho más estricto (antes 0.1f)
        jointTemporal.projectionAngle = 1f;       // Mucho más estricto (antes 5f)
        jointTemporal.enablePreprocessing = true;  // Ayuda a mantener estable la fuerza inicial
    }

    private void EliminarPivoteTemporal()
    {
        if (jointTemporal == null)
            return;

        Destroy(jointTemporal);
        jointTemporal = null;
    }

    // ============================================================
    // ESCALADO
    // ============================================================

    private void ProcesarEscalado()
    {
        float delta =
            velocidadEscala * Time.deltaTime;

        if (inputReader == null)
            return;

        Vector2 direccion =
            inputReader.scaleDirections;

        // ================================
        // NUEVO: Escalado con ratón
        // ================================

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRatonX;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRatonY;

        direccion.x += mouseX;
        direccion.y += mouseY;

        // Eje Y: aumentar o reducir longitud.

        if (direccion.y > 0.1f)
        {
            transform.localScale +=
                new Vector3(0f, 0f, delta);
        }
        else if (direccion.y < -0.1f)
        {
            float nuevaZ = Mathf.Max(
                transform.localScale.z - delta,
                escalaMinima
            );

            transform.localScale =
                new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    nuevaZ
                );
        }

        // Eje X: aumentar o reducir anchura.

        if (direccion.x > 0.1f)
        {
            float nuevaY = Mathf.Max(
                transform.localScale.y - delta,
                escalaMinima
            );

            transform.localScale =
                new Vector3(
                    transform.localScale.x,
                    nuevaY,
                    transform.localScale.z
                );
        }
        else if (direccion.x < -0.1f)
        {
            transform.localScale +=
                new Vector3(0f, delta, 0f);
        }

        if (
            SwordFloorGhost != null &&
            SwordFloorGhost.activeSelf
        )
        {
            SwordFloorGhost.transform.localScale =
                transform.localScale;

            SwordFloorGhost.transform.rotation =
                transform.rotation *
                Quaternion.Euler(0f, 90f, 0f);
        }
    }

    // ============================================================
    // COLISIONES DEL FANTASMA
    // ============================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Colision"))
            estaTocando = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Colision"))
            estaTocando = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Colision"))
            estaTocando = false;
    }
}