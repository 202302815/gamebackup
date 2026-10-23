using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampusLoadingScreen : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        SetProgress(0f);

        // Let the loading screen appear before loading the campus.
        yield return null;

        AsyncOperation loading =
            SceneManager.LoadSceneAsync("SampleScene");

        if (loading == null)
            yield break;

        loading.allowSceneActivation = false;

        while (loading.progress < 0.9f)
        {
            SetProgress(loading.progress / 0.9f);
            yield return null;
        }

        SetProgress(1f);

        // Display the completed bar before entering the campus.
        yield return null;

        loading.allowSceneActivation = true;
    }

    void SetProgress(float progress)
    {
        if (loadingBar != null)
            loadingBar.value = progress;

        if (loadingText != null)
        {
            loadingText.text =
                $"Loading campus… {Mathf.RoundToInt(progress * 100f)}%";
        }
    }
}