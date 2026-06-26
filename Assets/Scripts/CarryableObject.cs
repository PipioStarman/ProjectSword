using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarryableObject : MonoBehaviour
{
    [Header("Referencias")]
    public Rigidbody rb;
    public Collider[] colliders;

    [Header("Ajustes al coger")]
    public Vector3 posicionLocalAlCoger = Vector3.zero;
    public Vector3 rotacionLocalAlCoger = Vector3.zero;

    [Header("Estado")]
    public bool estaCogido = false;

    private Transform padreOriginal;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider>();

        padreOriginal = transform.parent;
    }

    public void Coger(Transform carryPoint)
    {
        estaCogido = true;

        padreOriginal = transform.parent;

        // --- NUEVO: apagamos físicas mientras está encima de la cabeza ---
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // --- NUEVO: evitamos que choque con el personaje mientras lo lleva ---
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }

        // --- NUEVO: lo pegamos al punto de la cabeza ---
        transform.SetParent(carryPoint);
        transform.localPosition = posicionLocalAlCoger;
        transform.localRotation = Quaternion.Euler(rotacionLocalAlCoger);
    }

    public void Soltar()
    {
        estaCogido = false;

        // --- NUEVO: lo despegamos del personaje ---
        transform.SetParent(padreOriginal);

        // --- NUEVO: reactivamos físicas ---
        rb.isKinematic = false;
        rb.useGravity = true;

        foreach (Collider col in colliders)
        {
            col.isTrigger = false;
        }
    }

    public void Tirar(Vector3 direccion, float fuerza)
    {
        Soltar();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direccion.normalized * fuerza, ForceMode.VelocityChange);
    }
    //private void FixedUpdate()
    //{
    //    rb.AddForce(Physics.gravity * rb.mass * 1.5f, ForceMode.Force);
    //}
}