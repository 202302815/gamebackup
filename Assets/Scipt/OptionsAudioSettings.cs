using UnityEngine;
using UnityEngine.UI;

public class OptionsAudioSettings : MonoBehaviour
{
    public GameMusicManager musicManager;
    public UISoundManager soundManager;

    public Slider musicSlider;
    public Slider effectsSlider;

    public Toggle musicMuteToggle;
    public Toggle effectsMuteToggle;

    public MainCameraPinchZoom zoomController;
    public Slider zoomSensitivitySlider;

    void OnEnable()
    {
        float musicDefault =
            musicManager != null ? musicManager.musicVolume : 0.25f;

        float effectsDefault =
            soundManager != null ? soundManager.effectsVolume : 0.5f;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("MusicVolume", musicDefault));
        }

        if (effectsSlider != null)
        {
            effectsSlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("EffectsVolume", effectsDefault));
        }

        if (musicMuteToggle != null)
        {
            musicMuteToggle.SetIsOnWithoutNotify(
                PlayerPrefs.GetInt("MusicMuted", 0) == 1);
        }

        if (effectsMuteToggle != null)
        {
            effectsMuteToggle.SetIsOnWithoutNotify(
                PlayerPrefs.GetInt("EffectsMuted", 0) == 1);
        }
        float zoomDefault =
            zoomController != null ? zoomController.sensitivity : 1f;

        if (zoomSensitivitySlider != null)
        {
            zoomSensitivitySlider.SetValueWithoutNotify(
                Mathf.Clamp(
                    PlayerPrefs.GetFloat("ZoomSensitivity", zoomDefault),
                    0.1f,
                    2f));
        }
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            PlayerPrefs.Save();
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}