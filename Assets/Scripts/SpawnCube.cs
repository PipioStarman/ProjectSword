using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    // 1. Arrastra desde el Inspector de Unity el Prefab que quieres crear (la espada, una caja, etc.)
    public GameObject objetoAPrefab;

    // 2. Arrastra un punto de referencia (un GameObject vacío) para saber DÓNDE crearlo
    public Transform puntoDeAparicion;

    void Update()
    {
        // Si pulsas la barra espaciadora...
        if (Input.GetKeyDown(KeyCode.L))
        {
            CrearObjeto();
        }
    }

    void CrearObjeto()
    {
        if (objetoAPrefab != null && puntoDeAparicion != null)
        {
           
            Instantiate(objetoAPrefab, puntoDeAparicion.position, puntoDeAparicion.rotation);
        }
        else
        {
            Debug.LogWarning("¡Te falta asignar el Prefab o el Punto de Aparición en el Inspector!");
        }
    }
}
