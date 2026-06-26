using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class JointSlidingDoor : MonoBehaviour
{
    [Header("Movimiento")]
    public float distanciaApertura = 3f;

    [Header("Detección por distancia")]
    public Transform player;

    // ================================
    // NUEVO: punto desde donde se mide la distancia
    // Si está vacío, usa transform.position
    // ================================
    public Transform detectionPoint;

    public float distanciaParaCerrar = 3f;
    public bool cerrarCuandoJugadorCerca = true;

    [Header("Joint Drive")]
    public float fuerzaMuelle = 800f;
    public float amortiguacion = 80f;
    public float fuerzaMaxima = 1000f;

    [Header("Debug")]
    public bool empezarAbierta = true;
    public bool jugadorCerca;

    // ================================
    // NUEVO: ver distancia real en inspector
    // ================================
    public float distanciaActual;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    private bool estaAbierta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        ConfigurarJoint();

        estaAbierta = empezarAbierta;
        AplicarObjetivo();
    }

    private void Update()
    {
        DetectarJugadorPorDistancia();

        if (!cerrarCuandoJugadorCerca)
            return;

        if (jugadorCerca)
            CerrarPuerta();
        else
            AbrirPuerta();
    }

    private void DetectarJugadorPorDistancia()
    {
        if (player == null)
        {
            jugadorCerca = false;
            distanciaActual = -1f;
            return;
        }

        // ================================
        // NUEVO:
        // Medimos desde detectionPoint si existe.
        // Si no, usamos la posición de la puerta.
        // ================================
        Vector3 origenDeteccion = detectionPoint != null
            ? detectionPoint.position
            : transform.position;

        distanciaActual = Vector3.Distance(origenDeteccion, player.position);

        jugadorCerca = distanciaActual <= distanciaParaCerrar;
    }

    private void ConfigurarJoint()
    {
        joint.configuredInWorldSpace = false;

        joint.axis = Vector3.right;
        joint.secondaryAxis = Vector3.up;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit limit = joint.linearLimit;
        limit.limit = distanciaApertura;
        joint.linearLimit = limit;

        JointDrive drive = joint.xDrive;
        drive.positionSpring = fuerzaMuelle;
        drive.positionDamper = amortiguacion;
        drive.maximumForce = fuerzaMaxima;
        joint.xDrive = drive;
    }

    public void AbrirPuerta()
    {
        if (estaAbierta)
            return;

        estaAbierta = true;
        AplicarObjetivo();
    }

    public void CerrarPuerta()
    {
        if (!estaAbierta)
            return;

        estaAbierta = false;
        AplicarObjetivo();
    }

    private void AplicarObjetivo()
    {
        if (joint == null)
            return;

        if (estaAbierta)
        {
            joint.targetPosition = new Vector3(-distanciaApertura, 0f, 0f);
        }
        else
        {
            joint.targetPosition = Vector3.zero;
        }
    }

    private void OnValidate()
    {
        if (joint == null)
            joint = GetComponent<ConfigurableJoint>();

        if (joint != null)
        {
            ConfigurarJoint();
            AplicarObjetivo();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origenDeteccion = detectionPoint != null
            ? detectionPoint.position
            : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origenDeteccion, distanciaParaCerrar);

        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origenDeteccion, player.position);
        }
    }
}