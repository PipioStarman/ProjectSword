using UnityEngine;

public class EnergyReceiver : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnergyDoor door;

    // ============================================================
    // NUEVO:
    // Aquí puedes añadir los 3 CROSS que quieres mover.
    // También admite más o menos de 3.
    // ============================================================
    [SerializeField] private JointRotatingObject[] rotatingObjects;

    [Header("Configuración")]
    [SerializeField] private bool closeWhenCubeRemoved = true;

    // ============================================================
    // NUEVO:
    // Decide si los CROSS vuelven a su posición inicial
    // cuando quitas el cubo de energía.
    // ============================================================
    [SerializeField] private bool resetObjectsWhenCubeRemoved = true;

    private EnergyCube currentCube;

    private void OnTriggerEnter(Collider other)
    {
        EnergyCube energyCube =
            other.GetComponentInParent<EnergyCube>();

        if (energyCube == null)
            return;

        if (currentCube != null)
            return;

        currentCube = energyCube;

        // Abre la puerta.
        if (door != null)
            door.OpenDoor();

        // ============================================================
        // NUEVO:
        // Activa todos los CROSS asignados.
        // ============================================================
        ActivateRotatingObjects();
    }

    private void OnTriggerExit(Collider other)
    {
        EnergyCube energyCube =
            other.GetComponentInParent<EnergyCube>();

        if (energyCube == null)
            return;

        if (energyCube != currentCube)
            return;

        currentCube = null;

        // Cierra la puerta al retirar el cubo.
        if (closeWhenCubeRemoved && door != null)
            door.CloseDoor();

        // ============================================================
        // NUEVO:
        // Devuelve todos los CROSS a su rotación inicial.
        // ============================================================
        if (resetObjectsWhenCubeRemoved)
            DeactivateRotatingObjects();
    }

    // ============================================================
    // NUEVO: activar todos los objetos giratorios
    // ============================================================

    private void ActivateRotatingObjects()
    {
        if (rotatingObjects == null)
            return;

        foreach (JointRotatingObject rotatingObject in rotatingObjects)
        {
            if (rotatingObject != null)
                rotatingObject.Activate();
        }
    }

    // ============================================================
    // NUEVO: desactivar todos los objetos giratorios
    // ============================================================

    private void DeactivateRotatingObjects()
    {
        if (rotatingObjects == null)
            return;

        foreach (JointRotatingObject rotatingObject in rotatingObjects)
        {
            if (rotatingObject != null)
                rotatingObject.Deactivate();
        }
    }
}