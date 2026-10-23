using UnityEngine;
using UnityEngine.EventSystems;

public class SprintButtonHold : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement playerMovement;
    public VehicleRoadRules vehicleRules;

    private CanvasGroup group;
    private int? heldPointer;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        bool riding =
            vehicleRules != null && vehicleRules.vehicleMode;

        bool available = !riding && Time.timeScale > 0f;

        group.alpha = available ? 1f : 0f;
        group.interactable = available;
        group.blocksRaycasts = available;

        if (!available)
            ReleaseSprint();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!group.interactable || heldPointer.HasValue)
            return;

        heldPointer = eventData.pointerId;

        if (playerMovement != null)
            playerMovement.StartSprint();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (heldPointer == eventData.pointerId)
            ReleaseSprint();
    }

    void ReleaseSprint()
    {
        heldPointer = null;

        if (playerMovement != null)
            playerMovement.StopSprint();
    }

    void OnDisable()
    {
        ReleaseSprint();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleaseSprint();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            ReleaseSprint();
    }
}