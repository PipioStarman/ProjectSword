using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class EnergyDoor : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Distancia que recorrerá la puerta sobre su eje local.")]
    [SerializeField] private float openingDistance = 3f;

    [Tooltip("Eje local sobre el que se desplazará la puerta.")]
    [SerializeField] private MovementAxis movementAxis = MovementAxis.X;

    [Header("Joint Drive")]
    [SerializeField] private float springForce = 800f;
    [SerializeField] private float damping = 80f;
    [SerializeField] private float maximumForce = 1000f;

    [Header("Comportamiento")]
    [SerializeField] private bool startOpen;
    [SerializeField] private bool invertDirection;

    [Header("Debug")]
    [SerializeField] private bool isOpen;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    private enum MovementAxis
    {
        X,
        Y,
        Z
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        ConfigureRigidbody();
        ConfigureJoint();

        isOpen = startOpen;
        ApplyTargetPosition();
    }

    // ============================================================
    // CONFIGURACIÓN DEL RIGIDBODY
    // ============================================================

    private void ConfigureRigidbody()
    {
        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // La puerta solo debe desplazarse, no girar.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // ============================================================
    // CONFIGURACIÓN DEL CONFIGURABLE JOINT
    // ============================================================

    private void ConfigureJoint()
    {
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null;

        /*
         * IMPORTANTE:
         * Al no tener connectedBody, connectedAnchor está en coordenadas
         * globales y mantiene el punto inicial de la puerta.
         */
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = transform.position;

        joint.configuredInWorldSpace = false;

        // El eje principal del ConfigurableJoint siempre será su X local.
        joint.axis = GetLocalMovementAxis();
        joint.secondaryAxis = GetSecondaryAxis();

        // Solo permitimos desplazamiento por el eje principal.
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit limit = joint.linearLimit;
        limit.limit = openingDistance;
        limit.bounciness = 0f;
        limit.contactDistance = 0.01f;
        joint.linearLimit = limit;

        JointDrive drive = joint.xDrive;
        drive.positionSpring = springForce;
        drive.positionDamper = damping;
        drive.maximumForce = maximumForce;
        joint.xDrive = drive;

        joint.enableCollision = false;
        joint.enablePreprocessing = true;

        // Ayuda a que la puerta no se descuadre bajo fuerzas grandes.
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.05f;
        joint.projectionAngle = 2f;
    }

    // ============================================================
    // ABRIR Y CERRAR
    // ============================================================

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;
        ApplyTargetPosition();
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;
        ApplyTargetPosition();
    }

    private void ApplyTargetPosition()
    {
        if (joint == null)
            return;

        float direction = invertDirection ? 1f : -1f;

        /*
         * Aunque visualmente elijas X, Y o Z, el eje libre del joint
         * se configura internamente como su eje X principal.
         */
        joint.targetPosition = isOpen
            ? new Vector3(direction * openingDistance, 0f, 0f)
            : Vector3.zero;

        rb?.WakeUp();
    }

    // ============================================================
    // EJES
    // ============================================================

    private Vector3 GetLocalMovementAxis()
    {
        switch (movementAxis)
        {
            case MovementAxis.Y:
                return Vector3.up;

            case MovementAxis.Z:
                return Vector3.forward;

            default:
                return Vector3.right;
        }
    }

    private Vector3 GetSecondaryAxis()
    {
        /*
         * axis y secondaryAxis no deben apuntar en la misma dirección.
         */
        switch (movementAxis)
        {
            case MovementAxis.Y:
                return Vector3.forward;

            case MovementAxis.Z:
                return Vector3.up;

            default:
                return Vector3.up;
        }
    }

    // ============================================================
    // ACTUALIZACIÓN EN EL INSPECTOR
    // ============================================================

    private void OnValidate()
    {
        openingDistance = Mathf.Max(0f, openingDistance);
        springForce = Mathf.Max(0f, springForce);
        damping = Mathf.Max(0f, damping);
        maximumForce = Mathf.Max(0f, maximumForce);

        if (!Application.isPlaying)
            return;

        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        ConfigureRigidbody();
        ConfigureJoint();
        ApplyTargetPosition();
    }
}