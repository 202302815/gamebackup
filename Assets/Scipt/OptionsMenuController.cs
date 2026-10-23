using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject startPanel;
    public HowToPlayManager tutorialManager;
    public PauseMenuManager pauseMenu;

    private bool openedFromPause;

    void Awake()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        openedFromPause = false;
        DisplayOptions();
    }

    public void OpenOptionsFromPause()
    {
        openedFromPause = true;
        DisplayOptions();
    }

    void DisplayOptions()
    {
        if (optionsPanel == null)
            return;

        if (!openedFromPause)
        {
            if (startPanel != null)
                startPanel.SetActive(true);

            SetStartMenuInteraction(false);
        }

        optionsPanel.SetActive(true);
        optionsPanel.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (openedFromPause)
        {
            if (pauseMenu != null)
                pauseMenu.ShowPausePanel();
        }
        else
        {
            SetStartMenuInteraction(true);
        }

        Time.timeScale = 0f;
    }

    public void ReplayTutorial()
    {
        if (tutorialManager == null)
        {
            Debug.LogWarning("Assign the tutorial manager.");
            return;
        }

        if (tutorialManager.ShowTutorialReplay())
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(false);

            Time.timeScale = 0f;
        }
    }

    public void ReturnFromTutorial()
    {
        // Preserve whether Options came from gameplay or the start menu.
        DisplayOptions();
    }

    void SetStartMenuInteraction(bool enabled)
    {
        if (startPanel == null)
            return;

        CanvasGroup group = startPanel.GetComponent<CanvasGroup>();

        if (group == null)
            group = startPanel.AddComponent<CanvasGroup>();

        group.interactable = enabled;
        group.blocksRaycasts = enabled;
    }
}