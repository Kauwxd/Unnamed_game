using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class musicManager : MonoBehaviour
{

    public static musicManager Instance;

    [SerializeField]
    private musicLibraryScript musicLibrary;
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField] 
    Slider musicVolumeSlider;
    [SerializeField]
    private float targetVolume = 1f;

    private void Start()
    {

        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1f);
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

    public void PlayMusic(string trackName, float fadeduration = 0.5f)
    {
       StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeduration));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(targetVolume, 0f, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, percent);
            yield return null;
        }
    }

    public void ChangeVolume()
    {
        targetVolume = musicVolumeSlider.value;
        musicSource.volume = targetVolume;
        Save();
    }

    private void Load()
    {
        targetVolume = PlayerPrefs.GetFloat("musicVolume");
        musicVolumeSlider.value = targetVolume;
        musicSource.volume = targetVolume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolumeSlider.value);
    }

}
