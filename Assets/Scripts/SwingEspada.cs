using System.Collections;
using UnityEngine;

public class SwingEspada: MonoBehaviour
{
    [Header("Rotaciones")]
    public Vector3 rotacionReposo;
    public Vector3 rotacionInicioAtaque;
    public Vector3 rotacionFinAtaque;

    [Header("Velocidad")]
    public float velocidadGolpe = 15f;
    public float velocidadRetorno = 8f;

    [Header("Deteccion de golpes")]
    public SwordHitbox hitbox;

    private bool estaAtacando = false;

    void Start()
    {
        transform.localRotation =
            Quaternion.Euler(rotacionReposo);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !estaAtacando)
        {
            StartCoroutine(CorutinaAtaqueCompleto());
        }
    }

    IEnumerator CorutinaAtaqueCompleto()
    {
        estaAtacando = true;

        Quaternion qReposo =
            Quaternion.Euler(rotacionReposo);

        Quaternion qInicio =
            Quaternion.Euler(rotacionInicioAtaque);

        Quaternion qFin =
            Quaternion.Euler(rotacionFinAtaque);

        // Colocar la espada al inicio
        transform.localRotation = qInicio;

        // Activar deteccion de golpes
        if (hitbox != null)
            hitbox.ActivarGolpe();

        // Ejecutar swing
        float tiempo = 0;

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadGolpe;

            transform.localRotation =
                Quaternion.Lerp(qInicio, qFin, tiempo);

            yield return null;
        }

        transform.localRotation = qFin;

        // Desactivar deteccion de golpes
        if (hitbox != null)
            hitbox.DesactivarGolpe();

        yield return new WaitForSeconds(0.05f);

        // Retorno a reposo
        tiempo = 0;

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadRetorno;

            transform.localRotation =
                Quaternion.Lerp(qFin, qReposo, tiempo);

            yield return null;
        }

        transform.localRotation = qReposo;

        estaAtacando = false;
    }
}