using UnityEngine;

public class FullMapPlayerMarker : MonoBehaviour
{
    public Transform player;
    public Camera mapCamera;
    public RectTransform mapDisplay;

    private RectTransform marker;

    void Awake()
    {
        marker = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (player == null || mapCamera == null || mapDisplay == null)
            return;

        Vector3 viewport =
            mapCamera.WorldToViewportPoint(player.position);

        Rect rect = mapDisplay.rect;

        marker.anchoredPosition = new Vector2(
            rect.xMin + viewport.x * rect.width,
            rect.yMin + viewport.y * rect.height
        );
    }
}