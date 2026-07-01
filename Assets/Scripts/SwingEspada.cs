using System.Collections;
using UnityEngine;

public class SwingEspadaPersonalizado : MonoBehaviour
{
    [Header("Rotaciones (Ángulos en X, Y, Z)")]
    public Vector3 rotacionReposo;
    public Vector3 rotacionInicioAtaque;
    public Vector3 rotacionFinAtaque;

    [Header("Velocidad")]
    public float velocidadGolpe = 15f;
    public float velocidadRetorno = 8f;

    private bool estaAtacando = false;

    void Start()
    {
        // Colocamos la espada en su posición de reposo al iniciar el juego
        transform.localRotation = Quaternion.Euler(rotacionReposo);
    }

    void Update()
    {
        // Detecta el clic izquierdo
        if (Input.GetMouseButtonDown(1) && !estaAtacando)
        {
            StartCoroutine(CorutinaAtaqueCompleto());
        }
    }

    IEnumerator CorutinaAtaqueCompleto()
    {
        estaAtacando = true;

        // Convertimos los Vector3 del Inspector a Quaternions para que Unity los entienda como rotaciones
        Quaternion qReposo = Quaternion.Euler(rotacionReposo);
        Quaternion qInicio = Quaternion.Euler(rotacionInicioAtaque);
        Quaternion qFin = Quaternion.Euler(rotacionFinAtaque);

        // 1. Ir rápidamente a la posición de inicio del ataque (opcional, por si quieres anticipación)
        // Si prefieres que empiece directo desde el reposo, puedes cambiar qInicio por qReposo abajo.
        transform.localRotation = qInicio;

        // 2. El Swing (De Inicio a Fin)
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadGolpe;
            transform.localRotation = Quaternion.Lerp(qInicio, qFin, tiempo);
            yield return null;
        }
        transform.localRotation = qFin;

        // Una pequeña pausa en el punto máximo del golpe (puedes borrarla si quieres que vuelva instantáneo)
        yield return new WaitForSeconds(0.05f);

        // 3. El Retorno (De Fin a Reposo)
        tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadRetorno;
            transform.localRotation = Quaternion.Lerp(qFin, qReposo, tiempo);
            yield return null;
        }
        transform.localRotation = qReposo;

        estaAtacando = false;
    }
}