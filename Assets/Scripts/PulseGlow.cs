using UnityEngine;

public class PulseGlow : MonoBehaviour
{
    private Material mat;
    private Color colorBase;

    [Header("Ajustes del Pulso")]
    public float velocidadVelocidad = 2f; // Qué tan rápido parpadea
    public float brilloMinimo = 0.5f;     // El brillo más bajo
    public float brilloMaximo = 2.5f;     // El brillo más alto (valores > 1 dan efecto HDR)

    private void Start()
    {
        // Obtenemos el material del propio objeto
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            // Guardamos el color original del Albedo como base del brillo
            colorBase = mat.color;

            // Le decimos a Unity que este material va a emitir luz
            mat.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (mat == null) return;

        // La función matemática Seno oscila entre -1 y 1. 
        // Con este cálculo la pasamos a un rango entre 0 y 1 para que sea más fácil de usar
        float oscilacion = (Mathf.Sin(Time.time * velocidadVelocidad) + 1f) / 2f;

        // Calculamos cuánto va a brillar en este frame exacto
        float intensidadActual = Mathf.Lerp(brilloMinimo, brilloMaximo, oscilacion);

        // Multiplicamos el color base por la intensidad HDR y se lo aplicamos a la emisión
        Color colorEmision = colorBase * intensidadActual;
        mat.SetColor("_EmissionColor", colorEmision);
    }
}