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

    private bool waitingForAvatar;

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

        playerMovement.StopButton();

        playerAnimator.runtimeAnimatorController = controller;
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

        SetGameplayUI(true);

        if (playerInput != null)
            playerInput.enabled = true;

        Time.timeScale = 1f;
    }
}