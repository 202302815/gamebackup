using UnityEngine;

public class BuildingDestination : MonoBehaviour
{
    public Transform walkingDestination;
    public Transform vehicleDestination;

    public Transform GetDestination(bool vehicleMode)
    {
        return vehicleMode
            ? vehicleDestination
            : walkingDestination;
    }
}