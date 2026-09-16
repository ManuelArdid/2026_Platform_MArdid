using UnityEngine;

public class SoundFXManager : Singleton<SoundFXManager>
{
    // ---------- UNITY EDITOR ---------- //
    [SerializeField] protected AudioSource SoundFXObject;
    [SerializeField] protected AudioSource SoundFXObject3D;


    // ---------- UNITY METHODS ---------- //
    protected void Awake()
    {
       DontDestroyOnLoad(gameObject);
    }

    // ---------- PUBLIC METHODS ---------- //

    /// <summary>
    /// Plays a sound effect clip at a given position with a specified volume.  
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    public void PlayFXClip(AudioClip clip, Vector3 position, float volume)
    {
        // Instantiate an AudioSource at the given position
        AudioSource audioSource = Instantiate(SoundFXObject, position, Quaternion.identity);

        _playUsingAudioSource(audioSource, clip, volume);
    }

    /// <summary>
    /// Plays a random sound effect clip from an array at a given position with a specified volume.
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    public void PlayRandomFXClip(AudioClip[] clips, Vector3 position, float volume)
    {
        int randomIndex = Random.Range(0, clips.Length);
        PlayFXClip(clips[randomIndex], position, volume);
    }

    /// <summary>
    /// Plays a sound effect clip at a given position with a specified volume in 3D.  
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    public void PlayFXClip3D(AudioClip clip, Vector3 position, float volume)
    {
        // Instantiate an AudioSource at the given position
        AudioSource audioSource = Instantiate(SoundFXObject3D, position, Quaternion.identity);

        _playUsingAudioSource(audioSource, clip, volume);
    }

    /// <summary>
    /// Plays a random sound effect clip from an array at a given position with a specified volume in 3D.
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    public void PlayRandomFXClip3D(AudioClip[] clips, Vector3 position, float volume)
    {
        int randomIndex = Random.Range(0, clips.Length);
        PlayFXClip3D(clips[randomIndex], position, volume);
    }

    // ---------- PRIVATE METHODS ---------- //
    private void _playUsingAudioSource(AudioSource source, AudioClip clip, float volume)
    {
        source.clip = clip;
        source.volume = volume;
        source.Play();

        // get length of clip
        float clipLength = source.clip.length;

        // Destroy the AudioSource object after the clip has finished playing
        Destroy(source.gameObject, clipLength);
    }

}