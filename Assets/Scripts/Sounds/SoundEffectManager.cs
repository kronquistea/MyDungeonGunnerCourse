using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]

public class SoundEffectManager : SingletonMonobehavior<SoundEffectManager>
{
    public int soundsVolume = 8;

    private void Start()
    {
        SetSoundsVolume(soundsVolume);
    }

    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        // Play sound using a sound gameobject and component from the object pool
        SoundEffect sound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        sound.SetSound(soundEffect);

        // Start playing the sound
        sound.gameObject.SetActive(true);

        // Disable sound after duration of sound clip
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }

    /// <summary>
    /// Disable sound effect object after it has played thus returning it to the object pool
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="soundDuration"></param>
    /// <returns>Coroutine for disabling the sound after it has finished playing</returns>
    private IEnumerator DisableSound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set sounds volume
    /// </summary>
    /// <param name="soundsVolume"></param>
    private void SetSoundsVolume(int soundsVolume)
    {
        float muteDecibels = -80f;

        if (soundsVolume == 0)
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume", muteDecibels);
        }
        else
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume", HelperUtilities.LinearToDecibels(soundsVolume));
        }
    }
}
