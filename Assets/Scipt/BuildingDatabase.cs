using System;
using UnityEngine;

[Serializable]
public class BuildingInfo
{
    public string id;
    public string name;
    public string description;
    public string services;
}

[Serializable]
public class BuildingInfoList
{
    public BuildingInfo[] buildings;
}

public class BuildingDatabase : MonoBehaviour
{
    private BuildingInfoList data;

    void Awake()
    {
        TextAsset json = Resources.Load<TextAsset>("buildings");

        if (json == null)
        {
            Debug.LogError(
                "Cannot find Assets/Resources/buildings.json.");
            return;
        }

        try
        {
            data = JsonUtility.FromJson<BuildingInfoList>(json.text);

            if (data == null || data.buildings == null)
            {
                Debug.LogError(
                    "JSON must contain a buildings array.");
                return;
            }

            Debug.Log(
                $"Loaded {data.buildings.Length} building(s) from JSON.");
        }
        catch (Exception error)
        {
            Debug.LogError("Cannot read building JSON: " + error.Message);
        }
    }

    public BuildingInfo GetBuilding(string buildingId)
    {
        if (data == null || data.buildings == null)
            return null;

        foreach (BuildingInfo building in data.buildings)
        {
            if (building != null && building.id == buildingId)
                return building;
        }

        return null;
    }
}