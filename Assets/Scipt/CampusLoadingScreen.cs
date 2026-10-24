using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampusLoadingScreen : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;
    public TMP_Text loadingTipText;

    [Min(0f)]
    public float minimumLoadingTime = 4f;

    [Min(0.5f)]
    public float tipChangeInterval = 2f;

    [TextArea(1, 3)]
    public string[] tips =
    {
        "Use the search menu to find a building.",
        "Follow the yellow trail on your minimap.",
        "Hold RUN to move faster while walking.",
        "Vehicle mode follows campus one-way roads.",
        "Tap the minimap to view the whole campus."
    };

    private int currentTip;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        SetProgress(0f);

        currentTip = 0;
        ShowTip();

        // Give the loading screen a frame to appear.
        yield return null;

        float startTime = Time.realtimeSinceStartup;
        float nextTipTime =
            startTime + Mathf.Max(0.5f, tipChangeInterval);

        AsyncOperation loading =
            SceneManager.LoadSceneAsync("SampleScene");

        if (loading == null)
            yield break;

        loading.allowSceneActivation = false;

        while (true)
        {
            float now = Time.realtimeSinceStartup;

            float actualProgress =
                Mathf.Clamp01(loading.progress / 0.9f);

            float elapsed = now - startTime;

            float timeProgress = minimumLoadingTime > 0f
                ? Mathf.Clamp01(elapsed / minimumLoadingTime)
                : 1f;

            SetProgress(Mathf.Min(actualProgress, timeProgress));

            if (now >= nextTipTime)
            {
                if (tips != null && tips.Length > 0)
                {
                    currentTip = (currentTip + 1) % tips.Length;
                    ShowTip();
                }

                nextTipTime =
                    now + Mathf.Max(0.5f, tipChangeInterval);
            }

            if (loading.progress >= 0.9f && timeProgress >= 1f)
                break;

            yield return null;
        }

        SetProgress(1f);
        yield return null;

        loading.allowSceneActivation = true;
    }

    void ShowTip()
    {
        if (loadingTipText == null)
            return;

        loadingTipText.text =
            tips != null && tips.Length > 0
                ? tips[currentTip]
                : "";
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