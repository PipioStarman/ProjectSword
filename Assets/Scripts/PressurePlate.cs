using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Referencia al cañón")]
    [SerializeField] private Cannon cannon;

    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool dispararSoloUnaVez = false;

    private bool yaDisparo;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (dispararSoloUnaVez && yaDisparo)
            return;

        cannon.Shoot();
        yaDisparo = true;
    }
}