using UnityEngine;
using PhysicsCharacterController;

public class PoleVaultController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform swordPivot;
    [SerializeField] private GameObject swordVisual;

    [Header("Input")]
    [SerializeField] private int mouseButton = 0;

    [Header("Launch Force")]
    [SerializeField] private float forwardForce = 25f;
    [SerializeField] private float upForce = 15f;

    [Header("Behaviour")]
    [SerializeField] private bool lockMovementWhilePreparing = true;
    [SerializeField] private float ignoreGroundTimeAfterLaunch = 0.15f;

    // =====================================================
    // NUEVO: comprobación física independiente del suelo
    // =====================================================
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Release Rotation")]
    [SerializeField] private float releaseRotationDuration = 0.25f;
    [SerializeField] private Vector3 releaseEndRotation = new Vector3(70f, 0f, 0f);
    [SerializeField]
    private AnimationCurve releaseRotationCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float returnRotationDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool isPreparingPoleVault;

    private Quaternion originalSwordPivotRotation;
    private Coroutine releaseRotationRoutine;

    private void Awake()
    {
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();

        if (swordPivot != null)
            originalSwordPivotRotation = swordPivot.localRotation;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButton))
        {
            TryStartPoleVault();
        }

        if (Input.GetMouseButton(mouseButton) && isPreparingPoleVault)
        {
            // =====================================================
            // NUEVO: si dejamos de tocar el suelo mientras cargamos,
            // cancelamos el pole vault
            // =====================================================
            if (!IsTouchingGround())
            {
                CancelPoleVault();
                return;
            }

            UpdateAnimatorPreparing();
        }

        if (Input.GetMouseButtonUp(mouseButton) && isPreparingPoleVault)
        {
            // =====================================================
            // NUEVO: comprobamos el suelo otra vez justo al soltar
            // =====================================================
            if (!IsTouchingGround())
            {
                CancelPoleVault();
                return;
            }

            ReleasePoleVault();
        }
    }

    private void TryStartPoleVault()
    {
        if (characterManager == null)
            return;

        // =====================================================
        // MODIFICADO: ahora necesita cumplir ambas condiciones
        // =====================================================
        if (!characterManager.CanStartPoleVault())
            return;

        if (!IsTouchingGround())
            return;

        if (releaseRotationRoutine != null)
        {
            StopCoroutine(releaseRotationRoutine);
            releaseRotationRoutine = null;
        }

        isPreparingPoleVault = true;

        if (swordVisual != null)
            swordVisual.SetActive(true);

        if (swordPivot != null)
            swordPivot.localRotation = originalSwordPivotRotation;

        if (lockMovementWhilePreparing)
            characterManager.SetVaultingState(true);

        if (animator != null)
            animator.SetBool("PreparingPoleVault", true);
    }

    private void ReleasePoleVault()
    {
        isPreparingPoleVault = false;

        if (animator != null)
        {
            animator.SetBool("PreparingPoleVault", false);
            animator.SetTrigger("PoleVaultJump");
        }

        Vector3 launchVelocity =
            characterManager.GetMoveForward() * forwardForce +
            Vector3.up * upForce;

        characterManager.IgnoreGroundFor(ignoreGroundTimeAfterLaunch);
        characterManager.Launch(launchVelocity);

        if (releaseRotationRoutine != null)
            StopCoroutine(releaseRotationRoutine);

        releaseRotationRoutine = StartCoroutine(AnimateSwordRelease());
    }

    // =====================================================
    // NUEVO: cancela correctamente la preparación
    // =====================================================
    private void CancelPoleVault()
    {
        isPreparingPoleVault = false;

        if (animator != null)
            animator.SetBool("PreparingPoleVault", false);

        if (lockMovementWhilePreparing && characterManager != null)
            characterManager.SetVaultingState(false);

        if (swordPivot != null)
            swordPivot.localRotation = originalSwordPivotRotation;
    }

    // =====================================================
    // NUEVO: devuelve true solamente cuando la esfera
    // está tocando una capa configurada como suelo
    // =====================================================
    private bool IsTouchingGround()
    {
        if (groundCheck == null)
            return false;

        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private System.Collections.IEnumerator AnimateSwordRelease()
    {
        if (swordPivot == null)
            yield break;

        Quaternion startRot = swordPivot.localRotation;
        Quaternion endRot = Quaternion.Euler(releaseEndRotation);

        float timer = 0f;

        while (timer < releaseRotationDuration)
        {
            timer += Time.deltaTime;

            float t = timer / releaseRotationDuration;
            float curvedT = releaseRotationCurve.Evaluate(t);

            swordPivot.localRotation = Quaternion.Slerp(
                startRot,
                endRot,
                curvedT
            );

            yield return null;
        }

        swordPivot.localRotation = endRot;

        if (swordVisual != null)
            swordVisual.SetActive(false);

        swordPivot.localRotation = originalSwordPivotRotation;

        releaseRotationRoutine = null;
    }

    private void UpdateAnimatorPreparing()
    {
        if (animator == null)
            return;

        animator.SetBool("PreparingPoleVault", isPreparingPoleVault);
    }

    // =====================================================
    // NUEVO: dibuja el Ground Check en la Scene View
    // =====================================================
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}