using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampusLoadingScreen : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;

    [Min(0f)]
    public float minimumLoadingTime = 4f;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        SetProgress(0f);

        yield return null;

        float startTime = Time.realtimeSinceStartup;

        AsyncOperation loading =
            SceneManager.LoadSceneAsync("SampleScene");

        if (loading == null)
            yield break;

        loading.allowSceneActivation = false;

        while (true)
        {
            float actualProgress =
                Mathf.Clamp01(loading.progress / 0.9f);

            float elapsed =
                Time.realtimeSinceStartup - startTime;

            float timeProgress = minimumLoadingTime > 0f
                ? Mathf.Clamp01(elapsed / minimumLoadingTime)
                : 1f;

            SetProgress(Mathf.Min(actualProgress, timeProgress));

            if (loading.progress >= 0.9f && timeProgress >= 1f)
                break;

            yield return null;
        }

        SetProgress(1f);
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