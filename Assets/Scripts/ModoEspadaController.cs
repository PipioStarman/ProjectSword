using UnityEngine;

public class ModoEspadaController : MonoBehaviour
{
    [Header("Referencias")]
    public Scaler espadaReal;
    public GameObject fakeSword;
    public PhysicsCharacterController.InputReader inputReader;

    [Header("Escalado de la FakeSword (Ctrl + F)")]
    public float velocidadEscalaFake = 2500f;
    public float escalaMinimaFake = 0.1f;

    private bool ctrlPulsadoAnterior;
    private bool escalarPulsadoAnterior;

    private void Start()
    {
        // Estado inicial: se ve la fake, la real está "guardada".
        if (espadaReal != null)
            espadaReal.gameObject.SetActive(false);

        if (fakeSword != null)
            fakeSword.SetActive(true);
    }

    private void Update()
    {
        bool ctrlAhora = Input.GetKey(KeyCode.LeftControl);
        bool escalarAhora = inputReader != null && inputReader.scale;

        // ------------------------------------------------------
        // CTRL pulsado: entrar en modo preview con la fake.
        // Al SOLTAR Ctrl ya no hacemos nada: la fake se queda
        // donde está, con la escala que tenga en ese momento.
        // ------------------------------------------------------
        if (ctrlAhora && !ctrlPulsadoAnterior)
        {
            EntrarModoFake();
        }

        // Escalar la fake mientras se mantiene Ctrl + F.
        if (ctrlAhora && escalarAhora)
        {
            EscalarFakeSword();
        }

        // ------------------------------------------------------
        // F pulsada SIN Ctrl (flanco de subida): sacar la espada
        // real. Solo se activa con una pulsación nueva de F, no
        // por soltar Ctrl mientras F sigue pulsada.
        // ------------------------------------------------------
        if (!ctrlAhora && escalarAhora && !escalarPulsadoAnterior)
        {
            EntrarModoReal();
        }

        ctrlPulsadoAnterior = ctrlAhora;
        escalarPulsadoAnterior = escalarAhora;
    }

    private void EntrarModoFake()
    {
        if (espadaReal != null)
            espadaReal.gameObject.SetActive(false);

        if (fakeSword != null)
            fakeSword.SetActive(true);
    }

    private void EntrarModoReal()
    {
        if (fakeSword != null)
            fakeSword.SetActive(false);

        if (espadaReal != null)
            espadaReal.gameObject.SetActive(true);
        // A partir de aquí, Scaler.Update() (ya activo) detecta
        // inputReader.scale == true y entra en su propio modo
        // fantasma, exactamente como antes.
    }

    private void EscalarFakeSword()
    {
        if (fakeSword == null || inputReader == null)
            return;

        Vector2 direccion = inputReader.scaleDirections;
        float delta = velocidadEscalaFake * Time.deltaTime;

        Vector3 escala = fakeSword.transform.localScale;

        if (direccion.y > 0.1f)
        {
            escala.y += delta;
        }
        else if (direccion.y < -0.1f)
        {
            escala.y = Mathf.Max(escala.y - delta, escalaMinimaFake);
        }

        if (direccion.x > 0.1f)
        {
            escala.x = Mathf.Max(escala.x - delta, escalaMinimaFake);
        }
        else if (direccion.x < -0.1f)
        {
            escala.x += delta;
        }

        fakeSword.transform.localScale = escala;
    }
}