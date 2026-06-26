using UnityEngine;
using PhysicsCharacterController;

public class SimpleSwordTrampoline : MonoBehaviour
{
    public float launchMultiplier = 1.5f;
    public float minUpSpeed = 3f;
    public float cooldown = 0.2f;
    public float ignoreGroundTime = 0.15f;

    private Rigidbody playerRb;
    private CharacterManager playerController;

    private float cooldownTimer;

    private Vector3 previousPosition;
    private Vector3 velocity;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.fixedDeltaTime;
        }

        if (playerRb == null)
            return;

        if (cooldownTimer > 0f)
            return;

        if (velocity.y > minUpSpeed)
        {
            Vector3 playerVelocity = playerRb.linearVelocity;

            playerVelocity.y = velocity.y * launchMultiplier;

            playerRb.linearVelocity = playerVelocity;

            if (playerController != null)
            {
                playerController.IgnoreGroundFor(ignoreGroundTime);
            }

            cooldownTimer = cooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterManager character = other.GetComponentInParent<CharacterManager>();

        if (character == null)
            return;

        playerController = character;
        playerRb = character.GetComponent<Rigidbody>();
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterManager character = other.GetComponentInParent<CharacterManager>();

        if (character != null && character == playerController)
        {
            playerController = null;
            playerRb = null;
        }
    }
}