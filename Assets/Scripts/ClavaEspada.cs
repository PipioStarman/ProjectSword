using UnityEngine;

public class ClavaEspada : MonoBehaviour
{
    [Header("Configuración del Raycast")]
    [SerializeField] private float distanciaDeteccion = 2.0f;
    [SerializeField] private LayerMask capaParedes;
    public Transform RaySpawnPoint;
    [Header("Objeto a Spamear")]
    [SerializeField] private GameObject objetoAPrefab;

    void Update()
    {
        // Ejecutamos la lógica, por ejemplo, al pulsar la tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            DetectarYSpawnear();
        }
    }

    private void DetectarYSpawnear()
    {
        // El rayo sale desde el centro del objeto hacia adelante
        Ray rayo = new Ray(RaySpawnPoint.position, transform.forward);
        RaycastHit hit;

        // Visualizar el rayo en el editor para debug
        Debug.DrawRay(RaySpawnPoint.position, transform.forward * distanciaDeteccion, Color.red, 1.0f);

        if (Physics.Raycast(rayo, out hit, distanciaDeteccion, capaParedes))
        {
            // Instanciar el objeto en el punto exacto del impacto
            // Usamos hit.normal para que el objeto se oriente según la inclinación de la pared
            Vector3 normalPared = hit.normal;
            Quaternion rotacionImpacto = Quaternion.LookRotation(normalPared);

            Instantiate(objetoAPrefab, hit.point, rotacionImpacto*objetoAPrefab.transform.rotation);

            Debug.Log("Objeto spawneado en: " + hit.point);
        }
    }
}
