using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MainCameraPinchZoom : MonoBehaviour
{
    public CinemachineCamera gameCamera;
    public CinemachineConfiner2D confiner;

    public float minimumSize = 5f;
    public float maximumSize = 14f;

    [Range(0.1f, 2f)]
    public float sensitivity = 1f;

    public GameObject[] blockingPanels;

    private float previousDistance;
    private int firstTouchId = -1;
    private int secondTouchId = -1;
    private bool gestureBlocked;

    private readonly List<RaycastResult> uiHits =
        new List<RaycastResult>();

    void Awake()
    {
        sensitivity = Mathf.Clamp(
            PlayerPrefs.GetFloat("ZoomSensitivity", sensitivity),
            0.1f,
            2f);
    }

    public void SetZoomSensitivity(float value)
    {
        sensitivity = Mathf.Clamp(value, 0.1f, 2f);

        PlayerPrefs.SetFloat("ZoomSensitivity", sensitivity);
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ResetGesture();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        ResetGesture();
    }

    void OnValidate()
    {
        minimumSize = Mathf.Max(0.1f, minimumSize);
        maximumSize = Mathf.Max(minimumSize, maximumSize);
    }

    void Update()
    {
        if (gameCamera == null || IsBlocked())
        {
            ResetGesture();
            return;
        }

        var touches = Touch.activeTouches;

        if (touches.Count != 2)
        {
            ResetGesture();
            return;
        }

        var first = touches[0];
        var second = touches[1];

        if (!first.isInProgress || !second.isInProgress)
        {
            ResetGesture();
            return;
        }

        Vector2 firstPosition = first.screenPosition;
        Vector2 secondPosition = second.screenPosition;

        float distance = Vector2.Distance(
            firstPosition, secondPosition);

        bool newPair =
            first.touchId != firstTouchId ||
            second.touchId != secondTouchId;

        if (newPair)
        {
            firstTouchId = first.touchId;
            secondTouchId = second.touchId;
            previousDistance = distance;

            gestureBlocked =
                IsOverUI(firstPosition) ||
                IsOverUI(secondPosition);

            return;
        }

        if (IsOverUI(firstPosition) || IsOverUI(secondPosition))
            gestureBlocked = true;

        // A gesture touching UI stays blocked until fingers lift.
        if (gestureBlocked)
            return;

        if (distance > 1f && previousDistance > 1f)
        {
            float ratio = previousDistance / distance;

            var lens = gameCamera.Lens;

            float size = Mathf.Clamp(
                lens.OrthographicSize *
                Mathf.Pow(ratio, sensitivity),
                minimumSize,
                maximumSize);

            if (!Mathf.Approximately(size, lens.OrthographicSize))
            {
                lens.OrthographicSize = size;
                gameCamera.Lens = lens;

                if (confiner != null)
                    confiner.InvalidateLensCache();
            }
        }

        previousDistance = distance;
    }

    bool IsBlocked()
    {
        if (Time.timeScale == 0f)
            return true;

        if (blockingPanels != null)
        {
            foreach (GameObject panel in blockingPanels)
            {
                if (panel != null && panel.activeInHierarchy)
                    return true;
            }
        }

        return false;
    }

    bool IsOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = position;

        uiHits.Clear();
        EventSystem.current.RaycastAll(pointer, uiHits);

        foreach (RaycastResult hit in uiHits)
        {
            if (hit.module is GraphicRaycaster)
                return true;
        }

        return false;
    }

    void ResetGesture()
    {
        previousDistance = 0f;
        firstTouchId = -1;
        secondTouchId = -1;
        gestureBlocked = false;
    }
}