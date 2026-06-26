using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlidingPhysicsDoor : MonoBehaviour
{
    [Header("Movimiento de la puerta")]
    public Vector3 direccionLocalMovimiento = Vector3.right;
    public float distanciaApertura = 3f;

    [Header("Timing")]
    public float tiempoAbierta = 2f;
    public float tiempoCerrada = 2f;

    [Header("Física")]
    public float velocidadMovimiento = 2f;
    public float distanciaParaConsiderarLlegada = 0.05f;
    public float fuerzaCorreccionRail = 25f;
    private bool haLlegadoAlObjetivo = false;
    [Header("Debug / Testing")]
    // ✅ NUEVO: permite cambiar valores en Play Mode y que se apliquen al momento
    public bool actualizarValoresEnRealtime = true;

    // ✅ NUEVO: dibuja el rail en la escena para ver por dónde se mueve
    public bool dibujarRail = true;

    private Rigidbody rb;

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;
    private Vector3 posicionObjetivo;

    private Vector3 direccionRailMundo;

    private bool quiereAbrirse = true;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        posicionCerrada = transform.position;

        // ✅ CAMBIADO: usamos una función para poder recalcularlo también en runtime
        RecalcularRailYPosiciones();

        posicionObjetivo = posicionCerrada;

        haLlegadoAlObjetivo = true;
    }

    private void Update()
    {
        // ✅ NUEVO: recalcula dirección/distancia en realtime mientras juegas
        if (actualizarValoresEnRealtime)
        {
            RecalcularRailYPosiciones();
        }

        // ✅ CAMBIADO:
        // Antes el timer contaba siempre.
        // Ahora solo cuenta cuando la puerta ya ha llegado al objetivo.
        if (!haLlegadoAlObjetivo)
        {
            return;
        }

        timer += Time.deltaTime;

        if (quiereAbrirse && timer >= tiempoCerrada)
        {
            quiereAbrirse = false;
            timer = 0f;
            haLlegadoAlObjetivo = false; // ✅ NUEVO

            posicionObjetivo = posicionAbierta;
        }
        else if (!quiereAbrirse && timer >= tiempoAbierta)
        {
            quiereAbrirse = true;
            timer = 0f;
            haLlegadoAlObjetivo = false; // ✅ NUEVO

            posicionObjetivo = posicionCerrada;
        }
    }

    private void FixedUpdate()
    {
        Vector3 desdeCerrada = rb.position - posicionCerrada;
        float distanciaEnRail = Vector3.Dot(desdeCerrada, direccionRailMundo);

        Vector3 posicionEnRail = posicionCerrada + direccionRailMundo * distanciaEnRail;

        Vector3 errorLateral = posicionEnRail - rb.position;
        rb.AddForce(errorLateral * fuerzaCorreccionRail, ForceMode.Acceleration);

        Vector3 haciaObjetivo = posicionObjetivo - rb.position;
        float distanciaHastaObjetivoEnRail = Vector3.Dot(haciaObjetivo, direccionRailMundo);

        if (Mathf.Abs(distanciaHastaObjetivoEnRail) < distanciaParaConsiderarLlegada)
        {
            // ✅ CAMBIADO: marcamos que la puerta ha llegado
            haLlegadoAlObjetivo = true;

            // ✅ CAMBIADO: frenamos la puerta al llegar
            Vector3 velocidadLateral = rb.linearVelocity - Vector3.Project(rb.linearVelocity, direccionRailMundo);
            rb.linearVelocity = velocidadLateral;

            return;
        }

        // ✅ NUEVO: si no está cerca, todavía no ha llegado
        haLlegadoAlObjetivo = false;

        float direccionMovimiento = Mathf.Sign(distanciaHastaObjetivoEnRail);

        Vector3 velocidadDeseada = direccionRailMundo * direccionMovimiento * velocidadMovimiento;

        rb.linearVelocity = velocidadDeseada;
    }

    // ✅ NUEVO: función separada para recalcular el rail cuando cambias valores en el inspector
    private void RecalcularRailYPosiciones()
    {
        if (direccionLocalMovimiento == Vector3.zero)
        {
            direccionLocalMovimiento = Vector3.right;
        }

        direccionRailMundo = transform.TransformDirection(direccionLocalMovimiento.normalized);

        posicionAbierta = posicionCerrada + direccionRailMundo * distanciaApertura;

        // ✅ NUEVO: si está en modo abrir, el objetivo se actualiza en realtime
        if (!quiereAbrirse)
        {
            posicionObjetivo = posicionAbierta;
        }
        else
        {
            posicionObjetivo = posicionCerrada;
        }
    }

    // ✅ NUEVO: dibuja la línea de movimiento en la Scene View
    private void OnDrawGizmos()
    {
        if (!dibujarRail) return;

        Vector3 origen = Application.isPlaying ? posicionCerrada : transform.position;

        Vector3 direccion = transform.TransformDirection(direccionLocalMovimiento.normalized);
        Vector3 destino = origen + direccion * distanciaApertura;

        Gizmos.DrawLine(origen, destino);
        Gizmos.DrawSphere(origen, 0.1f);
        Gizmos.DrawSphere(destino, 0.1f);
    }
}