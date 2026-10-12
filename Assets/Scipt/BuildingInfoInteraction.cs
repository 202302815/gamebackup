using System.Collections.Generic;
using UnityEngine;

public class BuildingInfoInteraction : MonoBehaviour
{
    public GameObject infoButton;
    public GameObject infoMenu;
    public BuildingInfoPanel infoPanel;

    private readonly List<Sign> nearbySigns = new List<Sign>();
    private Sign openedSign;

    void Awake()
    {
        if (infoMenu != null)
            infoMenu.SetActive(false);

        if (infoButton != null)
            infoButton.SetActive(false);
    }

    public void EnterSign(Sign sign)
    {
        if (!nearbySigns.Contains(sign))
            nearbySigns.Add(sign);

        RefreshButton();
    }

    public void LeaveSign(Sign sign)
    {
        nearbySigns.Remove(sign);

        if (openedSign == sign)
            CloseInfo();
        else
            RefreshButton();
    }

    public void OpenInfo()
    {
        if (nearbySigns.Count == 0 ||
            infoPanel == null ||
            infoMenu == null)
            return;

        Sign sign = nearbySigns[nearbySigns.Count - 1];

        if (!infoPanel.DisplayBuilding(sign.buildingId))
            return;

        openedSign = sign;
        infoMenu.SetActive(true);
        RefreshButton();
    }

    public void CloseInfo()
    {
        openedSign = null;

        if (infoMenu != null)
            infoMenu.SetActive(false);

        RefreshButton();
    }

    void RefreshButton()
    {
        if (infoButton != null)
        {
            bool panelOpen =
                infoMenu != null && infoMenu.activeSelf;

            infoButton.SetActive(
                nearbySigns.Count > 0 && !panelOpen);
        }
    }
}