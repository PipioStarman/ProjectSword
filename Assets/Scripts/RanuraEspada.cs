
using UnityEngine;

public class RanuraEspada : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Rigidbody rbJugador;
    public PhysicsCharacterController.CharacterManager controlador;
    public Scaler espadaReal;

    [Header("Configuración de huecos")]
    public Transform primerHueco;
    public int numeroHuecos = 10;
    public float separacionHuecos = 1f;

    [Header("Detección")]
    public float distanciaActivacion = 1.5f;
    public float anguloMaximo = 70f;

    [Header("Input")]
    public KeyCode teclaEscalado = KeyCode.F;

    private void Awake()
    {
        if (jugador != null)
        {
            if (rbJugador == null)
                rbJugador = jugador.GetComponent<Rigidbody>();

            if (controlador == null)
                controlador = jugador.GetComponent<
                    PhysicsCharacterController.CharacterManager>();
        }
    }

    private void LateUpdate()
    {
        if (!Input.GetKeyDown(teclaEscalado))
            return;

        if (Input.GetKey(KeyCode.LeftControl))
            return;

        if (jugador == null ||
            primerHueco == null ||
            espadaReal == null)
            return;

        if (!espadaReal.gameObject.activeInHierarchy)
            return;

        if (numeroHuecos <= 0 || separacionHuecos <= 0f)
            return;

        // Encontrar el hueco más cercano.
        Vector3 desplazamiento =
            jugador.position - primerHueco.position;

        float distanciaHorizontal = Vector3.Dot(
            desplazamiento,
            primerHueco.right
        );

        int indiceHueco = Mathf.RoundToInt(
            distanciaHorizontal / separacionHuecos
        );

        indiceHueco = Mathf.Clamp(
            indiceHueco,
            0,
            numeroHuecos - 1
        );

        Vector3 posicionObjetivo =
            primerHueco.position +
            primerHueco.right *
            (indiceHueco * separacionHuecos);

        // Comprobar distancia.
        if (Vector3.Distance(
            jugador.position,
            posicionObjetivo
        ) > distanciaActivacion)
            return;

        // Comprobar si mira hacia los barrotes.
        Vector3 adelanteJugador = Vector3.ProjectOnPlane(
            jugador.forward, Vector3.up
        );

        Vector3 adelanteObjetivo = Vector3.ProjectOnPlane(
            primerHueco.forward, Vector3.up
        );

        if (adelanteJugador.sqrMagnitude > 0.001f &&
            adelanteObjetivo.sqrMagnitude > 0.001f)
        {
            float angulo = Vector3.Angle(
                adelanteJugador,
                adelanteObjetivo
            );

            if (angulo > anguloMaximo)
                return;
        }

        ColocarJugador(posicionObjetivo);
    }

    private void ColocarJugador(Vector3 posicion)
    {
        // Copiar orientación horizontal del hueco.
        float anguloY = primerHueco.eulerAngles.y;
        Quaternion rotacion = Quaternion.Euler(0f, anguloY, 0f);

        // IMPORTANTE:
        // Actualizar la dirección interna del controlador.
        if (controlador != null)
        {
            controlador.targetAngle = anguloY;

            // Girar inmediatamente el modelo visual.
            if (controlador.characterModel != null)
            {
                controlador.characterModel.transform.rotation =
                    rotacion;
            }
        }

        // Colocar el Rigidbody.
        if (rbJugador != null)
        {
            rbJugador.linearVelocity = Vector3.zero;
            rbJugador.angularVelocity = Vector3.zero;

            rbJugador.position = posicion;
            rbJugador.rotation = rotacion;
        }
        else
        {
            jugador.SetPositionAndRotation(
                posicion,
                rotacion
            );
        }

        Physics.SyncTransforms();
    }

    private void OnDrawGizmosSelected()
    {
        if (primerHueco == null ||
            separacionHuecos <= 0f)
            return;

        for (int i = 0; i < numeroHuecos; i++)
        {
            Vector3 posicion =
                primerHueco.position +
                primerHueco.right *
                (i * separacionHuecos);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(posicion, 0.12f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                posicion,
                distanciaActivacion
            );

            // Azul: dirección a la que mirará el personaje.
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(
                posicion,
                primerHueco.forward * 0.6f
            );
        }
    }
}
