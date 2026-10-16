using UnityEngine;
using UnityEngine.Events;

public class ObjetivoGolpeEspada : MonoBehaviour
{
    [Header("Configuracion")]
    public bool soloUnaVez = true;

    [Header("Evento al recibir golpe")]
    public UnityEvent onGolpe;

    private bool activado = false;

    public void RecibirGolpe()
    {
        if (soloUnaVez && activado)
            return;

        activado = true;

        onGolpe?.Invoke();
    }
}