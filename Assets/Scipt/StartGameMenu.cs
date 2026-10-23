using UnityEngine;
using UnityEngine.InputSystem;

public class StartGameMenu : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gateSelectionPanel;

    public PlayerMovement playerMovement;
    public PlayerInput playerInput;

    public Transform gate1Spawn;
    public Transform gate2Spawn;
    public Transform gate3Spawn;

    public GameObject[] gameplayUI;
    public GameObject avatarSelectionPanel;
    public Animator playerAnimator;
    public RuntimeAnimatorController boyController;
    public RuntimeAnimatorController girlController;
    public VehicleRoadRules vehicleRules;
    public RuntimeAnimatorController boyScooterController;
    public RuntimeAnimatorController girlScooterController;
    public GameObject exitConfirmationPanel;
    public HowToPlayManager tutorialManager;
    public GameMusicManager musicManager;

    public GameObject aboutPanel;
    public UnityEngine.UI.ScrollRect aboutScrollView;

    private bool selectedGirl;

    private bool waitingForAvatar;
    public PauseMenuManager pauseMenu;
    private bool exitFromGameplay;

    void Start()
    {
        Time.timeScale = 0f;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (gateSelectionPanel != null)
            gateSelectionPanel.SetActive(false);

        if (playerInput != null)
            playerInput.enabled = false;

        SetGameplayUI(false);

        waitingForAvatar = false;

        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(false);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (gateSelectionPanel == null)
        {
            Debug.LogWarning("Assign Gate Selection Panel.");
            return;
        }

        if (startPanel != null)
            startPanel.SetActive(false);

        gateSelectionPanel.SetActive(true);
    }

    public void ChooseGate1()
    {
        SpawnAtGate(gate1Spawn);
    }

    public void ChooseGate2()
    {
        SpawnAtGate(gate2Spawn);
    }

    public void ChooseGate3()
    {
        SpawnAtGate(gate3Spawn);
    }
    public void OpenAbout()
    {
        if (aboutPanel == null)
            return;

        aboutPanel.SetActive(true);
        aboutPanel.transform.SetAsLastSibling();

        if (aboutScrollView != null)
        {
            Canvas.ForceUpdateCanvases();
            aboutScrollView.StopMovement();
            aboutScrollView.verticalNormalizedPosition = 1f;
        }

        if (startPanel != null)
        {
            CanvasGroup menu = startPanel.GetComponent<CanvasGroup>();

            if (menu != null)
            {
                menu.interactable = false;
                menu.blocksRaycasts = false;
            }
        }
    }

    public void CloseAbout()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (startPanel != null)
        {
            CanvasGroup menu = startPanel.GetComponent<CanvasGroup>();

            if (menu != null)
            {
                menu.interactable = true;
                menu.blocksRaycasts = true;
            }
        }
    }
    void SpawnAtGate(Transform spawnPoint)
    {
        if (spawnPoint == null ||
            playerMovement == null ||
            avatarSelectionPanel == null)
        {
            Debug.LogWarning(
                "Assign the player, gate spawn point, and avatar panel.");
            return;
        }

        playerMovement.StopButton();

        Rigidbody2D rb =
            playerMovement.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = spawnPoint.position;
        }

        Vector3 position = spawnPoint.position;
        position.z = 0f;
        playerMovement.transform.position = position;

        Physics2D.SyncTransforms();

        if (startPanel != null)
            startPanel.SetActive(false);

        if (gateSelectionPanel != null)
            gateSelectionPanel.SetActive(false);

        SetGameplayUI(false);

        if (playerInput != null)
            playerInput.enabled = false;

        Time.timeScale = 0f;
        waitingForAvatar = true;
        avatarSelectionPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetGameplayUI(bool visible)
    {
        foreach (GameObject item in gameplayUI)
        {
            if (item != null)
                item.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void ChooseBoy()
    {
        ApplyAvatar(boyController);
    }

    public void ChooseGirl()
    {
        ApplyAvatar(girlController);
    }

    void ApplyAvatar(RuntimeAnimatorController controller)
    {
        if (!waitingForAvatar)
            return;

        if (playerAnimator == null ||
            controller == null ||
            playerMovement == null)
        {
            Debug.LogWarning("Assign the player Animator and avatar controllers.");
            return;
        }

        selectedGirl = controller == girlController;

        RuntimeAnimatorController selectedController =
            GetSelectedController();

        if (selectedController == null)
        {
            Debug.LogWarning("Assign the selected avatar's controller.");
            return;
        }

        playerMovement.StopButton();

        playerAnimator.runtimeAnimatorController = selectedController;
        playerAnimator.Rebind();

        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetFloat("InputX", 0f);
        playerAnimator.SetFloat("InputY", 0f);
        playerAnimator.SetFloat("LastInputX", 0f);
        playerAnimator.SetFloat("LastInputY", -1f);

        // Display the chosen avatar even while gameplay is paused.
        playerAnimator.Update(0f);

        waitingForAvatar = false;
        avatarSelectionPanel.SetActive(false);

        waitingForAvatar = false;
        avatarSelectionPanel.SetActive(false);

        if (tutorialManager != null && !tutorialManager.HasSeenTutorial)
        {
            Time.timeScale = 0f;
            SetGameplayUI(false);

            if (playerInput != null)
                playerInput.enabled = false;

            tutorialManager.ShowTutorial();
        }
        else
        {
            BeginExploring();
        }
    }

    public void BeginExploring()
    {
        if (musicManager != null)
            musicManager.PlayGameplayMusic();
        if (playerMovement != null)
            playerMovement.StopButton();

        SetGameplayUI(true);

        if (playerInput != null)
            playerInput.enabled = true;

        Time.timeScale = 1f;
    }
    RuntimeAnimatorController GetSelectedController()
    {
        bool riding = vehicleRules != null && vehicleRules.vehicleMode;

        if (selectedGirl)
            return riding ? girlScooterController : girlController;

        return riding ? boyScooterController : boyController;
    }

    public void RefreshAvatarForMode()
    {
        if (playerAnimator == null)
            return;

        RuntimeAnimatorController controller = GetSelectedController();

        if (controller == null)
        {
            Debug.LogWarning("Assign all walking and scooter controllers.");
            return;
        }

        if (playerAnimator.runtimeAnimatorController == controller)
            return;

        // Remember movement and facing while changing appearance.
        bool walking = playerAnimator.GetBool("isWalking");
        float inputX = playerAnimator.GetFloat("InputX");
        float inputY = playerAnimator.GetFloat("InputY");
        float lastX = playerAnimator.GetFloat("LastInputX");
        float lastY = playerAnimator.GetFloat("LastInputY");

        playerAnimator.runtimeAnimatorController = controller;

        playerAnimator.SetBool("isWalking", walking);
        playerAnimator.SetFloat("InputX", inputX);
        playerAnimator.SetFloat("InputY", inputY);
        playerAnimator.SetFloat("LastInputX", lastX);
        playerAnimator.SetFloat("LastInputY", lastY);

        playerAnimator.Update(0f);
    }

    public void ShowExitConfirmation()
    {
        exitFromGameplay = false;
        OpenExitConfirmation();
    }

    public void ShowGameplayExitConfirmation()
    {
        exitFromGameplay = true;
        OpenExitConfirmation();
    }

    void OpenExitConfirmation()
    {
        if (exitConfirmationPanel == null)
            return;

        exitConfirmationPanel.SetActive(true);
        exitConfirmationPanel.transform.SetAsLastSibling();

        if (!exitFromGameplay && startPanel != null)
        {
            CanvasGroup menu = startPanel.GetComponent<CanvasGroup>();

            if (menu != null)
            {
                menu.interactable = false;
                menu.blocksRaycasts = false;
            }
        }
    }

    public void CancelExit()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (exitFromGameplay)
        {
            if (pauseMenu != null)
                pauseMenu.ShowPausePanel();
        }
        else if (startPanel != null)
        {
            CanvasGroup menu = startPanel.GetComponent<CanvasGroup>();

            if (menu != null)
            {
                menu.interactable = true;
                menu.blocksRaycasts = true;
            }
        }
    }

    public void BackToStartMenu()
    {
        if (gateSelectionPanel != null)
            gateSelectionPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(true);

        Time.timeScale = 0f;
    }
}