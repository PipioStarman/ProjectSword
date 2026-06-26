using UnityEngine;

/// <summary>
/// Coloca este componente en GameObjects vacíos posicionados sobre el balancín.
/// La espada hará snap al más cercano dentro del radio definido en Scaler.
/// La rotación del SnapPoint define cómo quedará orientada la espada al encajar.
/// </summary>
public class SnapPoint : MonoBehaviour
{
    [Header("Debug visual en editor")]
    public Color colorGizmo = new Color(0f, 1f, 0.5f, 0.4f);
    public float radioVisualizacion = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = colorGizmo;
        Gizmos.DrawSphere(transform.position, radioVisualizacion);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}
