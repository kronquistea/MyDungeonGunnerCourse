using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]

public class MusicManager : SingletonMonobehavior<MusicManager>
{
    private AudioSource musicAudioSource = null;
    private AudioClip currentAudioClip = null;
    private Coroutine fadeOutMusicCoroutine;
    private Coroutine fadeInMusicCoroutine;
    public int musicVolume = 10;

    protected override void Awake()
    {
        // Load components
        base.Awake();
        musicAudioSource = GetComponent<AudioSource>();

        // Start with music off
        GameResources.Instance.musicOffSnapshot.TransitionTo(0f);
    }

    private void Start()
    {
        // Check if the player has set a different volume than the default volume in the previous game session
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            musicVolume = PlayerPrefs.GetInt("musicVolume");
        }

        SetMusicVolume(musicVolume);
    }

    private void OnDisable()
    {
        // Store music volume between game sessions
        PlayerPrefs.SetInt("musicVolume", musicVolume);
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    /// <param name="musicVolume"></param>
    private void SetMusicVolume(int musicVolume)
    {
        // Define a variable representing muted audio in decibels
        float muteDecibels = -80f;

        // Check if the volume is set to 0
        if (musicVolume == 0)
        {
            // Mute the music volume
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", muteDecibels);
        }
        else
        {
            // Set the music volume to the desired volume in decibels
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", HelperUtilities.LinearToDecibels(musicVolume));
        }
    }

    /// <summary>
    /// Increase the music volume 
    /// </summary>
    public void IncreaseMusicVolume()
    {
        // Set max music volume 
        int maxMusicVolume = 20;

        // Check if the current music volume is at max or not
        if (musicVolume >= maxMusicVolume)
        {
            return;
        }

        // Increase the music volume
        musicVolume++;

        // Set the music volume
        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Decrease the music volume
    /// </summary>
    public void DecreaseMusicVolume()
    {
        // Set min music volume
        int minMusicVolume = 0;

        // Check if the current volume is at lowest or not
        if (musicVolume <= minMusicVolume)
        {
            return;
        }

        // Decrease the music volume
        musicVolume--;

        // Set the music volume
        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Play the desired music track
    /// </summary>
    /// <param name="musicTrack"></param>
    /// <param name="fadeOutTime"></param>
    /// <param name="fadeInTime"></param>
    public void PlayMusic(MusicTrackSO musicTrack, float fadeOutTime = Settings.musicFadeOutTime, float fadeInTime = Settings.musicFadeInTime)
    {
        // Play music track
        StartCoroutine(PlayMusicRoutine(musicTrack, fadeOutTime, fadeInTime));
    }

    /// <summary>
    /// Play music for room routine
    /// </summary>
    /// <param name="musicTrack"></param>
    /// <param name="fadeOutTime"></param>
    /// <param name="fadeInTime"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator PlayMusicRoutine(MusicTrackSO musicTrack, float fadeOutTime, float fadeInTime)
    {
        // Clear fade out music coroutine
        if (fadeOutMusicCoroutine != null)
        {
            StopCoroutine(fadeOutMusicCoroutine);
        }

        // Clear fade in music coroutine
        if (fadeInMusicCoroutine != null)
        {
            StopCoroutine(fadeInMusicCoroutine);
        }

        // Check if the music track has changed
        if (musicTrack.musicClip != currentAudioClip)
        {
            // Set the new music clip
            currentAudioClip = musicTrack.musicClip;

            // Fade out the old music clip
            yield return fadeOutMusicCoroutine = StartCoroutine(FadeOutMusic(fadeOutTime));

            // Fade in the new music clip
            yield return fadeInMusicCoroutine = StartCoroutine(FadeInMusic(musicTrack, fadeInTime));
        }

        yield return null;
    }

    /// <summary>
    /// Fade out music routine
    /// </summary>
    /// <param name="fadeOutTime"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator FadeOutMusic(float fadeOutTime)
    {
        // Decrease the volume over the specified amount of time (since the music low snapshot really just has a lower volume)
        GameResources.Instance.musicLowSnapshot.TransitionTo(fadeOutTime);

        yield return new WaitForSeconds(fadeOutTime);
    }

    /// <summary>
    /// Fade in music routine
    /// </summary>
    /// <param name="musicTrack"></param>
    /// <param name="fadeInTime"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator FadeInMusic(MusicTrackSO musicTrack, float fadeInTime)
    {
        // Set the source clip and volume then play the clip
        musicAudioSource.clip = musicTrack.musicClip;
        musicAudioSource.volume = musicTrack.musicVolume;
        musicAudioSource.Play();

        // Increase the volume over the specified amount of time (since the music on full snapshot really just has a "normal" volume)
        GameResources.Instance.musicOnFullSnapshot.TransitionTo(fadeInTime);

        yield return new WaitForSeconds(fadeInTime);
    }
}
