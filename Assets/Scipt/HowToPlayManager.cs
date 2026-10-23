using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HowToPlayManager : MonoBehaviour
{
    [Serializable]
    public class TutorialPage
    {
        public string title;

        [TextArea(2, 5)]
        public string description;

        public Sprite image;
    }

    public GameObject tutorialPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text pageNumberText;
    public Image tutorialImage;

    public Button backButton;
    public TMP_Text nextButtonText;

    public TutorialPage[] pages = new TutorialPage[5];

    public UnityEvent onTutorialFinished;
    public UnityEvent onTutorialReplayFinished;
    private bool replaying;

    private const string TutorialSeenKey = "UniTopia_TutorialSeen";
    private int currentPage;
    private bool tutorialOpen;

    public bool HasSeenTutorial =>
        PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;

    void Awake()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    public void ShowTutorial()
    {
        if (tutorialPanel == null ||
            titleText == null ||
            descriptionText == null ||
            pageNumberText == null ||
            tutorialImage == null ||
            backButton == null ||
            nextButtonText == null ||
            pages == null ||
            pages.Length == 0)
        {
            Debug.LogWarning("Assign all tutorial fields and pages.");
            return;
        }

        foreach (TutorialPage page in pages)
        {
            if (page == null)
            {
                Debug.LogWarning("A tutorial page is missing.");
                return;
            }
        }

        currentPage = 0;
        replaying = false;
        tutorialOpen = true;

        tutorialPanel.SetActive(true);
        tutorialPanel.transform.SetAsLastSibling();

        DisplayPage();
    }

    public void NextPage()
    {
        if (!tutorialOpen)
            return;

        if (currentPage == pages.Length - 1)
        {
            FinishTutorial();
            return;
        }

        currentPage++;
        DisplayPage();
    }

    public void PreviousPage()
    {
        if (!tutorialOpen || currentPage == 0)
            return;

        currentPage--;
        DisplayPage();
    }

    public void SkipTutorial()
    {
        if (tutorialOpen)
            FinishTutorial();
    }

    void DisplayPage()
    {
        TutorialPage page = pages[currentPage];

        titleText.text = page.title;
        descriptionText.text = page.description;
        pageNumberText.text = $"{currentPage + 1} / {pages.Length}";

        tutorialImage.sprite = page.image;
        tutorialImage.preserveAspect = true;
        tutorialImage.gameObject.SetActive(page.image != null);
        tutorialImage.color = Color.white;

        backButton.interactable = currentPage > 0;

        nextButtonText.text = currentPage == pages.Length - 1
            ? (replaying ? "Return to Options" : "Start Exploring")
            : "Next";
    }

    void FinishTutorial()
    {
        if (!tutorialOpen)
            return;

        tutorialOpen = false;
        tutorialPanel.SetActive(false);

        if (replaying)
        {
            replaying = false;
            onTutorialReplayFinished.Invoke();
        }
        else
        {
            PlayerPrefs.SetInt(TutorialSeenKey, 1);
            PlayerPrefs.Save();

            onTutorialFinished.Invoke();
        }
    }

    [ContextMenu("Reset Tutorial Seen Flag")]
    void ResetTutorialSeenFlag()
    {
        PlayerPrefs.DeleteKey(TutorialSeenKey);
        PlayerPrefs.Save();

        Debug.Log("Tutorial flag reset.");
    }
    public bool ShowTutorialReplay()
    {
        ShowTutorial();

        if (!tutorialOpen)
            return false;

        replaying = true;
        DisplayPage();
        return true;
    }
}