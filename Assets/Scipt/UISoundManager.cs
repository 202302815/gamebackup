using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UISoundManager : MonoBehaviour
{
    public AudioClip buttonClick;
    public AudioClip arrivalSound;
    [Range(0f, 1f)]
    public float effectsVolume = 0.5f;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        effectsVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat("EffectsVolume", effectsVolume));

        source.volume = effectsVolume;
        source.mute = PlayerPrefs.GetInt("EffectsMuted", 0) == 1;
    }

    public void PlayButtonClick()
    {
        if (buttonClick != null)
            source.PlayOneShot(buttonClick);
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);

        if (source != null)
            source.volume = effectsVolume;

        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
    }
    public void PlayArrivalSound()
    {
        if (arrivalSound != null)
            source.PlayOneShot(arrivalSound);
    }
    public void SetEffectsMuted(bool muted)
    {
        if (source != null)
            source.mute = muted;

        PlayerPrefs.SetInt("EffectsMuted", muted ? 1 : 0);
    }
}