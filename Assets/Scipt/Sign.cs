using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    public BuildingInfoInteraction interaction;
    public string buildingId = "library";

    private readonly HashSet<Collider2D> playerColliders =
        new HashSet<Collider2D>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerColliders.Add(other) &&
            playerColliders.Count == 1 &&
            interaction != null)
        {
            interaction.EnterSign(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!playerColliders.Remove(other))
            return;

        if (playerColliders.Count == 0 && interaction != null)
            interaction.LeaveSign(this);
    }

    void OnDisable()
    {
        playerColliders.Clear();

        if (interaction != null)
            interaction.LeaveSign(this);
    }
}