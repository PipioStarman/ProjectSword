using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private Collider hitbox;

    private HashSet<ObjetivoGolpeEspada> golpeados =
        new HashSet<ObjetivoGolpeEspada>();

    void Awake()
    {
        hitbox = GetComponent<Collider>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;
    }

    public void ActivarGolpe()
    {
        golpeados.Clear();
        hitbox.enabled = true;
    }

    public void DesactivarGolpe()
    {
        hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        ComprobarGolpe(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ComprobarGolpe(other);
    }

    private void ComprobarGolpe(Collider other)
    {
        ObjetivoGolpeEspada objetivo =
            other.GetComponentInParent<ObjetivoGolpeEspada>();

        if (objetivo == null) return;

        // Evita golpear el mismo objeto varias veces
        // durante un único ataque.
        if (!golpeados.Add(objetivo)) return;

        objetivo.RecibirGolpe();
    }
}