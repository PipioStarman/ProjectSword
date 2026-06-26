using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class JointRotatingObject : MonoBehaviour
{
    private enum EjeRotacion
    {
        X,
        Y,
        Z
    }

    [Header("Rotación")]

    [Tooltip("Eje local alrededor del cual girará el objeto.")]
    [SerializeField] private EjeRotacion ejeRotacion = EjeRotacion.Y;

    [Tooltip("Grados que girará al activarse.")]
    [SerializeField] private float anguloActivado = 180f;

    [Tooltip("Invierte la dirección del giro.")]
    [SerializeField] private bool invertirDireccion;

    [Header("Joint Drive")]

    [SerializeField] private float fuerzaMuelle = 800f;

    [Tooltip("Sube este valor para evitar balanceo y rebotes.")]
    [SerializeField] private float amortiguacion = 150f;

    [SerializeField] private float fuerzaMaxima = 2000f;

    [Header("Estabilidad")]

    [Tooltip("Resistencia adicional a la rotación.")]
    [SerializeField] private float angularDrag = 8f;

    [Tooltip("Velocidad angular máxima del objeto.")]
    [SerializeField] private float velocidadAngularMaxima = 10f;

    [Header("Estado inicial")]

    [SerializeField] private bool empezarActivado;

    [Header("Debug")]

    [SerializeField] private bool estaActivado;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        ConfigurarRigidbody();
        ConfigurarJoint();

        estaActivado = empezarActivado;
        AplicarRotacionObjetivo();
    }

    private void FixedUpdate()
    {
        ActualizarDrive();
    }

    // ============================================================
    // RIGIDBODY
    // ============================================================

    private void ConfigurarRigidbody()
    {
        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // ============================================================
        // NUEVO:
        // Evita que conserve demasiado impulso angular.
        // ============================================================

        rb.angularDamping = angularDrag;
        rb.maxAngularVelocity = velocidadAngularMaxima;

        rb.constraints = RigidbodyConstraints.None;
    }

    // ============================================================
    // CONFIGURABLE JOINT
    // ============================================================

    private void ConfigurarJoint()
    {
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null;

        joint.anchor = Vector3.zero;
        joint.connectedAnchor = transform.position;

        joint.configuredInWorldSpace = false;

        // No puede desplazarse.
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        // ============================================================
        // NUEVO:
        // El eje elegido se convierte en el eje X principal del joint.
        // ============================================================

        joint.axis = ObtenerEjeLocal();
        joint.secondaryAxis = ObtenerEjeSecundario();

        // ============================================================
        // MODIFICADO:
        // Solo permitimos girar por el eje principal.
        // Los otros dos quedan completamente bloqueados.
        // ============================================================

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        joint.rotationDriveMode = RotationDriveMode.XYAndZ;

        ActualizarDrive();

        joint.enableCollision = false;
        joint.enablePreprocessing = true;

        joint.projectionMode =
            JointProjectionMode.PositionAndRotation;

        joint.projectionDistance = 0.01f;
        joint.projectionAngle = 1f;
    }

    private void ActualizarDrive()
    {
        if (joint == null)
            return;

        // ============================================================
        // MODIFICADO:
        // Como solo gira por Angular X, usamos angularXDrive.
        // ============================================================

        JointDrive drive = joint.angularXDrive;

        drive.positionSpring = fuerzaMuelle;
        drive.positionDamper = amortiguacion;
        drive.maximumForce = fuerzaMaxima;

        joint.angularXDrive = drive;

        if (rb != null)
        {
            rb.angularDamping = angularDrag;
            rb.maxAngularVelocity = velocidadAngularMaxima;
        }
    }

    // ============================================================
    // ROTACIÓN OBJETIVO
    // ============================================================

    private void AplicarRotacionObjetivo()
    {
        if (joint == null)
            return;

        float direccion = invertirDireccion ? -1f : 1f;

        float angulo = estaActivado
            ? anguloActivado * direccion
            : 0f;

        /*
         * Como el eje elegido se ha convertido internamente
         * en el eje X del ConfigurableJoint, rotamos alrededor de X.
         */
        Quaternion rotacionObjetivo =
            Quaternion.AngleAxis(angulo, Vector3.right);

        joint.targetRotation =
            Quaternion.Inverse(rotacionObjetivo);

        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();
    }

    // ============================================================
    // EJES
    // ============================================================

    private Vector3 ObtenerEjeLocal()
    {
        switch (ejeRotacion)
        {
            case EjeRotacion.X:
                return Vector3.right;

            case EjeRotacion.Z:
                return Vector3.forward;

            default:
                return Vector3.up;
        }
    }

    private Vector3 ObtenerEjeSecundario()
    {
        /*
         * El eje secundario no puede apuntar en la misma dirección
         * que el eje principal.
         */
        switch (ejeRotacion)
        {
            case EjeRotacion.X:
                return Vector3.up;

            case EjeRotacion.Z:
                return Vector3.up;

            default:
                return Vector3.forward;
        }
    }

    // ============================================================
    // MÉTODOS PÚBLICOS
    // ============================================================

    public void SetActivado(bool activado)
    {
        if (estaActivado == activado)
            return;

        estaActivado = activado;
        AplicarRotacionObjetivo();
    }

    public void Activate()
    {
        SetActivado(true);
    }

    public void Deactivate()
    {
        SetActivado(false);
    }

    public void Activar()
    {
        SetActivado(true);
    }

    public void Desactivar()
    {
        SetActivado(false);
    }

    // ============================================================
    // INSPECTOR
    // ============================================================

    private void OnValidate()
    {
        fuerzaMuelle = Mathf.Max(0f, fuerzaMuelle);
        amortiguacion = Mathf.Max(0f, amortiguacion);
        fuerzaMaxima = Mathf.Max(0f, fuerzaMaxima);
        angularDrag = Mathf.Max(0f, angularDrag);
        velocidadAngularMaxima =
            Mathf.Max(0.1f, velocidadAngularMaxima);

        if (!Application.isPlaying)
            return;

        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        ConfigurarRigidbody();
        ConfigurarJoint();
        AplicarRotacionObjetivo();
    }

    // ============================================================
    // DEBUG
    // ============================================================

    [ContextMenu("Test Activar")]
    private void TestActivar()
    {
        SetActivado(true);
    }

    [ContextMenu("Test Desactivar")]
    private void TestDesactivar()
    {
        SetActivado(false);
    }
}