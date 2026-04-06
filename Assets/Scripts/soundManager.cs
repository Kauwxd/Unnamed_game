using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class soundManager : MonoBehaviour
{
    public static soundManager Instance;

    [SerializeField]
    private soundLibraryScript soundLibrary;

    [SerializeField]
    private AudioSource soundSource;

    [SerializeField]
    Slider SFXVolumeSlider;

    private void Start()
    {
       
        if(!PlayerPrefs.HasKey("SFXVolume"))
        {
            PlayerPrefs.SetFloat("SFXVolume", 1f);
            Load();
        }
        else
        {
            Load();
        }

    }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySound(string soundName)
    {
        AudioClip clip = soundLibrary.GetClipFromName(soundName);

        if (clip != null)
        {
            soundSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    public void ChangeVolume()
    {
        soundSource.volume = SFXVolumeSlider.value;
        Save();
    }

    private void Load()
    {
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("SFXVolume", SFXVolumeSlider.value);
    }


}