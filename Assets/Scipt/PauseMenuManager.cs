using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject inputBlocker;

    public PlayerMovement playerMovement;
    public PlayerInput playerInput;

    public StartGameMenu startMenu;
    public OptionsMenuController optionsMenu;

    public bool IsPaused { get; private set; }

    private bool inputWasEnabled;

    void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (inputBlocker != null)
            inputBlocker.SetActive(false);
    }

    public void PauseGame()
    {
        if (IsPaused || Time.timeScale == 0f)
            return;

        if (pausePanel == null || inputBlocker == null)
        {
            Debug.LogWarning("Assign the pause panel and input blocker.");
            return;
        }

        inputWasEnabled = playerInput != null && playerInput.enabled;

        if (playerInput != null)
            playerInput.enabled = false;

        if (playerMovement != null)
        {
            playerMovement.StopButton();

            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        IsPaused = true;
        Time.timeScale = 0f;

        ShowPausePanel();
    }

    public void ShowPausePanel()
    {
        if (!IsPaused)
            return;

        if (inputBlocker != null)
        {
            inputBlocker.SetActive(true);
            inputBlocker.transform.SetAsLastSibling();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (inputBlocker != null)
            inputBlocker.SetActive(false);

        if (playerMovement != null)
            playerMovement.StopButton();

        if (playerInput != null)
            playerInput.enabled = inputWasEnabled;

        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        if (!IsPaused || optionsMenu == null)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        optionsMenu.OpenOptionsFromPause();
    }

    public void RequestExit()
    {
        if (!IsPaused || startMenu == null)
            return;

        if (startMenu.exitConfirmationPanel == null)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        startMenu.ShowGameplayExitConfirmation();
    }

    public void ReturnToStart()
    {
        if (!IsPaused)
            return;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.buildIndex < 0)
        {
            Debug.LogWarning("Add this scene to the Build Profiles scene list.");
            return;
        }

        PlayerPrefs.Save();

        // Reload resets the session, including routes and avatar selection.
        SceneManager.LoadScene(scene.buildIndex);
    }
}