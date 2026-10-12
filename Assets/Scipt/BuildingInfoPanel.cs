using TMPro;
using UnityEngine;

public class BuildingInfoPanel : MonoBehaviour
{
    public BuildingDatabase database;
    public TMP_Text titleText;
    public TMP_Text detailsText;

    public bool DisplayBuilding(string buildingId)
    {
        if (database == null || titleText == null || detailsText == null)
        {
            Debug.LogWarning("Assign the database and panel text fields.");
            return false;
        }

        BuildingInfo building = database.GetBuilding(buildingId);

        if (building == null)
        {
            Debug.LogWarning("Building ID not found: " + buildingId);
            return false;
        }

        titleText.text = building.name;
        detailsText.text =
            building.description +
            "\n\nServices:\n" +
            building.services;

        return true;
    }
}