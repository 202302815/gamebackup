using UnityEngine;
using UnityEngine.InputSystem;

public class FullMapController : MonoBehaviour
{
    public GameObject fullMapPanel;
    public Camera fullMapCamera;
    public PlayerMovement playerMovement;
    public PlayerInput playerInput;

    private bool isOpen;
    private bool inputWasEnabled;
    private float previousTimeScale;

    void Awake()
    {
        if (fullMapPanel != null)
            fullMapPanel.SetActive(false);

        if (fullMapCamera != null)
            fullMapCamera.enabled = false;
    }

    public void OpenMap()
    {
        if (isOpen || Time.timeScale == 0f)
            return;

        if (fullMapPanel == null || fullMapCamera == null)
        {
            Debug.LogWarning("Assign the full map panel and camera.");
            return;
        }

        previousTimeScale = Time.timeScale;
        inputWasEnabled = playerInput != null && playerInput.enabled;

        if (playerInput != null)
            playerInput.enabled = false;

        StopPlayer();

        isOpen = true;
        Time.timeScale = 0f;

        fullMapCamera.enabled = true;
        fullMapPanel.SetActive(true);
        fullMapPanel.transform.SetAsLastSibling();
    }

    public void CloseMap()
    {
        if (!isOpen)
            return;

        fullMapPanel.SetActive(false);
        fullMapCamera.enabled = false;

        StopPlayer();

        if (playerInput != null)
            playerInput.enabled = inputWasEnabled;

        Time.timeScale = previousTimeScale;
        isOpen = false;
    }

    void StopPlayer()
    {
        if (playerMovement == null)
            return;

        playerMovement.StopButton();

        Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}