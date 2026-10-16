
using UnityEngine;

public class PlataformaPorProximidad : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private GameObject plataforma;
    [SerializeField] private Transform puntoDeteccion;

    [Header("Distancias")]
    [SerializeField] private float distanciaDesaparecer = 3f;
    [SerializeField] private float distanciaReaparecer = 4f;

    [Header("Configuración")]
    [SerializeField] private bool ignorarAltura = true;

    private bool desaparecida;

    private void Start()
    {
        if (jugador == null || plataforma == null)
        {
            Debug.LogError(
                "Faltan referencias en PlataformaPorProximidad.",
                this
            );
            enabled = false;
            return;
        }

        if (puntoDeteccion == null)
            puntoDeteccion = transform;

        desaparecida = !plataforma.activeSelf;

        ActualizarPlataforma();
    }

    private void Update()
    {
        ActualizarPlataforma();
    }

    private void ActualizarPlataforma()
    {
        Vector3 diferencia =
            jugador.position - puntoDeteccion.position;

        if (ignorarAltura)
            diferencia.y = 0f;

        float distancia = diferencia.magnitude;

        if (!desaparecida &&
            distancia <= distanciaDesaparecer)
        {
            plataforma.SetActive(false);
            desaparecida = true;
        }
        else if (desaparecida &&
                 distancia >= distanciaReaparecer)
        {
            plataforma.SetActive(true);
            desaparecida = false;
        }
    }

    private void OnValidate()
    {
        distanciaDesaparecer =
            Mathf.Max(0f, distanciaDesaparecer);

        distanciaReaparecer =
            Mathf.Max(
                distanciaDesaparecer + 0.1f,
                distanciaReaparecer
            );
    }

    private void OnDrawGizmosSelected()
    {
        Transform centro =
            puntoDeteccion != null ? puntoDeteccion : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            centro.position,
            distanciaDesaparecer
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            centro.position,
            distanciaReaparecer
        );
    }
}
