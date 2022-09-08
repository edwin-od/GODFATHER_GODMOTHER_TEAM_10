using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sounds[] allSounds;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("There is more than one AudioManager in the scene");
            Destroy(gameObject);
        }
        instance = this;
        InitializeAllClips();
    }

    public void PauseOrUnpauseAllClips(bool that)
    {
        if (that)
        {
            allSounds[0].source.Play();
        }
        else
        {
            allSounds[0].source.Pause();
        }
    }

    /// <summary>
    /// Create an audio source for each clip and set it with the right parameter
    /// </summary>
    void InitializeAllClips()
    {
        foreach (Sounds s in allSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    public void PlayClip(string name)
    {
        if (name == "")
        {
            Debug.LogWarning("Enter a clip name !");
            return;
        }
        Sounds s = Array.Find(allSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("The clip " + name + " doesn't exist !");
            return;
        }
        s.source.Play();
    }
}
