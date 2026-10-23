using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameMusicManager : MonoBehaviour
{
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.25f;

    private AudioSource musicSource;

    void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat("MusicVolume", musicVolume));

        musicSource.volume = musicVolume;
        musicSource.mute = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
    }

    void Start()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        PlayTrack(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayTrack(gameplayMusic);
    }

    void PlayTrack(AudioClip track)
    {
        if (track == null)
        {
            Debug.LogWarning("Assign the music clips.");
            return;
        }

        // Avoid restarting the same track.
        if (musicSource.clip == track && musicSource.isPlaying)
            return;

        musicSource.clip = track;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    public void SetMusicMuted(bool muted)
    {
        if (musicSource != null)
            musicSource.mute = muted;

        PlayerPrefs.SetInt("MusicMuted", muted ? 1 : 0);
    }
}