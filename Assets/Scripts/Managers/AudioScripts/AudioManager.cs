using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannle { Master,Music,Sfx};
    
    public float masterVolumePercent { get; private set;}
    public float musicVolumePercent { get; private set;}
    public float sfxVolumePercent { get; private set;}

    [SerializeField] AudioSource sfx2DSource;

    //AudioSource[] musicSources;
    //int activeMusicSourceIndex;

    Transform audioListenerTransform;
    Transform playerTransform;
    Sounds sounds;


    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sounds = GetComponent<Sounds>();
            
            audioListenerTransform = GetComponentInChildren<AudioListener>().transform;

            if(!PlayerPrefs.HasKey("MasterVolume") 
                || !PlayerPrefs.HasKey("MusicVolume") 
                || !PlayerPrefs.HasKey("SfxVolume"))
            {
                PlayerPrefs.SetFloat("MasterVolume", 1);
                PlayerPrefs.SetFloat("MusicVolume", 1);
                PlayerPrefs.SetFloat("SfxVolume", 1);
            }
            masterVolumePercent = PlayerPrefs.GetFloat("MasterVolume");
            musicVolumePercent = PlayerPrefs.GetFloat("MusicVolume");
            sfxVolumePercent = PlayerPrefs.GetFloat("SfxVolume");

        }
        
    }
    
    private void Update()
    {
        if(playerTransform != null)
        {
            audioListenerTransform.position = playerTransform.position;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
         
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
         
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        sfx2DSource.volume = sfxVolumePercent * masterVolumePercent;
        playerTransform = GameManager.Instance.transform;
    }

    private void Play3DSound(AudioClip clip, Vector3 position, float maxDistance = 700f, float volume = 1)
    {
        var source3D = LeanPool.Spawn(sfx2DSource, position, Quaternion.identity);
        source3D.name = "3D Sound";
        source3D.clip = clip;
        source3D.spatialBlend = 1f;
        source3D.volume = volume;
        source3D.rolloffMode = AudioRolloffMode.Linear;
        source3D.maxDistance = maxDistance;
        source3D.Play();
        LeanPool.Despawn(source3D,Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale);
    }
    
    public void SetVolume(float volumePercent, AudioChannle channle)
    {
        switch (channle)
        {
            case AudioChannle.Master:
                masterVolumePercent = volumePercent;
                break;

            case AudioChannle.Music:
                musicVolumePercent = volumePercent;
                break;

            case AudioChannle.Sfx:
                sfxVolumePercent = volumePercent;
                break;
        }
        
        PlayerPrefs.SetFloat("MasterVolume", masterVolumePercent);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumePercent);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlaySound(AudioClip clip, Vector3 pos, float maxDistance = 700)
    {
        if (clip != null)
        {
            Play3DSound(clip, pos, maxDistance,sfxVolumePercent * masterVolumePercent);
        }
    }
    

    public void PlaySound(string soundName, Vector3 pos, float maxDistance = 700f)
    {
        PlaySound(sounds.GetAudioFromName(soundName), pos, maxDistance);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sounds.GetAudioFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }
    
}
