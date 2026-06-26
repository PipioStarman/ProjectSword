using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform carryPoint;
    public Transform puntoDeteccion;
    

    [Header("Detección")]
    public float radioDeteccion = 1.2f;
    public LayerMask capaObjetosCogibles;

    [Header("Input")]
    public KeyCode teclaAccion = KeyCode.E;

    [Header("Lanzamiento")]
    public float fuerzaTiro = 8f;
    public float fuerzaTiroVertical = 1.5f;

    [Header("Estado")]
    public CarryableObject objetoActual;

    private void Update()
    {
        if (Input.GetKeyDown(teclaAccion))
        {
            if (objetoActual == null)
            {
                // Si no llevo nada, intento coger algo.
                IntentarCogerObjeto();
            }
            else
            {
                // Si ya llevo algo, lo tiro.
                TirarObjeto();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (objetoActual != null)
            {
                SoltarObjeto();
            }
        }
    }

    private void IntentarCogerObjeto()
    {
        Collider[] hits = Physics.OverlapSphere(
            puntoDeteccion.position,
            radioDeteccion,
            capaObjetosCogibles,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0)
            return;

        CarryableObject objetoMasCercano = null;
        float distanciaMasCercana = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            CarryableObject carryable = hit.GetComponentInParent<CarryableObject>();

            if (carryable == null)
                continue;

            if (carryable.estaCogido)
                continue;

            float distancia = Vector3.Distance(transform.position, carryable.transform.position);

            if (distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                objetoMasCercano = carryable;
            }
        }

        if (objetoMasCercano == null)
            return;

        objetoActual = objetoMasCercano;
        objetoActual.Coger(carryPoint);
    }

    private void SoltarObjeto()
    {
        objetoActual.Soltar();
        objetoActual = null;
    }

    private void TirarObjeto()
    {
        Vector3 direccion;

        
            direccion = transform.forward;
        

        // --- NUEVO: pequeño empuje hacia arriba para que no vaya tan plano ---
        direccion += Vector3.up * fuerzaTiroVertical;

        objetoActual.Tirar(direccion, fuerzaTiro);
        objetoActual = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoDeteccion == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(puntoDeteccion.position, radioDeteccion);
    }
}