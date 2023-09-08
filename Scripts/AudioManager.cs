using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public struct SoundParameters
{
    //inspector parameters
    [Range(0, 1)]
    public float Volume;
    [Range(-3, 3)]
    public float Pitch;
    public bool Loop;
}

[System.Serializable()]
public class Sound
{
    [SerializeField] string name; //gets name of sound
    public string Name { get { return name; } }

    [SerializeField] AudioClip clip; //gets the sound clip
    public AudioClip Clip { get { return clip; } }

    [SerializeField] SoundParameters parameters; //gets the parameters above
    public SoundParameters Parameters { get { return parameters; } }

    [HideInInspector]
    public AudioSource Source;

    public void Play() //plays the source clip
    {
        Source.clip = Clip;

        Source.volume = Parameters.Volume;
        Source.pitch = Parameters.Pitch;
        Source.loop = Parameters.Loop;

        Source.Play();
    }

    public void Stop() //stops the sound
    {
        Source.Stop();
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] Sound[] sounds; //creates sound array
    [SerializeField] AudioSource sourcePrefabs;

    [SerializeField] string startupTrack; //sets the BG sound

    private void Awake()
    {
        if (Instance != null) //makes sure no duplicates of AudioManager exists
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        InitSounds();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(startupTrack) != true) //plays the startup sound on start up
        {
            PlaySound(startupTrack);
        }
    }

    void InitSounds()
    {
        foreach (var sound in sounds) //Loops through the sounds and gets the right one
        {
            AudioSource source = (AudioSource)Instantiate(sourcePrefabs, gameObject.transform);
            source.name = sound.Name;

            sound.Source = source;
        }
    }

    public void PlaySound(string name) //makes sure there is a sound and plays it
    {
        var sound = GetSound(name);

        if (sound != null)
        {
            sound.Play();
        }
        else
        {
            Debug.LogWarning("Sound by this name: " + name + " is not found! Issue occurs in AudioManager.PlaySound.");
        }
    }

    public void StopSound(string name) //makes sure there is a sound playing and stops it
    {
        var sound = GetSound(name); 

        if (sound != null)
        {
            sound.Stop();
        }
        else
        {
            Debug.LogWarning("Sound by this name: " + name + " is not found! Issue occurs in AudioManager.StopSound.");
        }
    }

    Sound GetSound(string name) 
    {
        foreach (var sound in sounds) //loops through the sound names and grabs the right one
        {
            if(sound.Name == name)
            {
                return sound;
            }
        }
        return null;
    }

}
