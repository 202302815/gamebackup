using UnityEngine;

public class MiniMapUI : MonoBehaviour
{
    public GameObject fullMapPanel;

    public void OpenFullMap()
    {
        fullMapPanel.SetActive(true);
    }

    public void CloseFullMap()
    {
        fullMapPanel.SetActive(false);
    }
}