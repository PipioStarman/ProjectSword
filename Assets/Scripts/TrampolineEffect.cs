using UnityEngine;

public class TrampolineEffect : MonoBehaviour
{
    [Header("Configuración de Rotación (Grados)")]
    public Vector3 rotationOffset = new Vector3(20, 0, 0);
    public float bendSpeed = 8f;
    public float returnSpeed = 4f;
    public AnimationCurve bendCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Vector3 initialEuler;
    private float timer = 0f;
    private bool isObjectOnTop = false;

    [Header("Ajustes de Salto")]
    public float jumpForce = 15f;
    public float thresholdToJump = 0.8f;
    private bool hasJumped = false;

    private Rigidbody objectRigidbody; // 👈 Renombrado

    void Start()
    {
        initialEuler = transform.localEulerAngles;
    }

    void Update()
    {
        if (isObjectOnTop)
        {
            timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * bendSpeed);

            if (timer >= thresholdToJump && !hasJumped)
            {
                ApplyJump();
            }
        }
        else
        {
            timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * returnSpeed);
            if (timer == 0) hasJumped = false;
        }

        float curveValue = bendCurve.Evaluate(timer);
        Vector3 currentEuler = new Vector3(
            Mathf.LerpAngle(initialEuler.x, initialEuler.x + rotationOffset.x, curveValue),
            Mathf.LerpAngle(initialEuler.y, initialEuler.y + rotationOffset.y, curveValue),
            Mathf.LerpAngle(initialEuler.z, initialEuler.z + rotationOffset.z, curveValue));

        transform.localEulerAngles = currentEuler;
    }

    void ApplyJump()
    {
        if (objectRigidbody != null)
        {
            objectRigidbody.linearVelocity = Vector3.zero;
            objectRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            hasJumped = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 👇 Cualquier objeto con Rigidbody activa el trampolín
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            objectRigidbody = rb;
            isObjectOnTop = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 👇 Solo desactiva si es el mismo objeto que entró
        if (other.GetComponent<Rigidbody>() == objectRigidbody)
        {
            isObjectOnTop = false;
            objectRigidbody = null;
        }
    }
}