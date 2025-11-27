using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{

    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider volumeSlider2;
    private AudioClip currentMusic = null;

    [Header("--------- Audio Source ---------")]
    
    [SerializeField]
    AudioSource musicSource;
    
    [SerializeField]
    AudioSource SFXSource;

    [Header("--------- Music  ---------")]

    public AudioClip mainMenuMusic;
    public AudioClip mainMenuMusic1;
    public AudioClip mainMenuMusic2;
    public AudioClip mainMenuMusic3;

    [Header("--------- Player  ---------")]

    public AudioClip playerShoot;
    public AudioClip playerWalking;
    public AudioClip playerRunning;
    public AudioClip playerHit;
    public AudioClip playerDie;
    public AudioClip money;
    public AudioClip moneyPayOut;
    public AudioClip click;


    [Header("--------- Custommer  ---------")]

    public AudioClip openShop;
    public AudioClip customer;
    public AudioClip soul;

    [Header("--------- Boss  ---------")]

    public AudioClip bossLose;
    public AudioClip bossHit;
    public AudioClip bossFightMusic;


    [Header("--------- Environment ---------")]
    
    public AudioClip midnight;
    public AudioClip morning;
    public AudioClip city;
    public AudioClip village;

    private void Start()
    {
        //musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        //SFXSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 1);
            PlayerPrefs.SetFloat("SFXVolume", 1);
        }

        Load();
    }
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        SFXSource.volume = volume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
        PlayerPrefs.SetFloat("SFXVolume", SFXSource.volume);
    }

    private void Load()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        volumeSlider.value = musicSource.volume;
        volumeSlider2.value = SFXSource.volume;
    }

    public void ChangeMusicVolume()
    {
        musicSource.volume = volumeSlider.value;
        Save();
    }

    public void ChangeSFXVolume()
    {
        SFXSource.volume = volumeSlider2.value;
        Save();
    }

    public void PlaySFX(AudioClip clip, bool loop = true)
    {
        SFXSource.clip = clip;
        SFXSource.loop = loop;
        SFXSource.Play();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }



    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }
}


