using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{

    #region Singleton
    public static SFXManager instance;
    
    void SetSingleton(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Debug.LogWarning("Two instances of SFXManager found! Destorying this gameobject!");
            Destroy(gameObject);
        }
    }
    #endregion

    #region Pool
    [SerializeField] int prewarmNum;

    List<AudioSource> pooledSources = new List<AudioSource>();
    #endregion

    [SerializeField] AudioMixerGroup uiGroup;

    [SerializeField] AudioClip[] clips;

    void Awake(){
        SetSingleton();

        if(instance != this) return;

        for (int i = 0; i < prewarmNum; i++)
        {
            AudioSource a = gameObject.AddComponent<AudioSource>();
            a.outputAudioMixerGroup = uiGroup;
            pooledSources.Add(a);
        }
    }

    public void StartSound(int id){
        if(clips[id] == null) return;

        AudioSource currentSource;

        if(pooledSources.Count >= 1){
            currentSource = pooledSources[0];
            pooledSources.Remove(currentSource);
        }else{
            currentSource = gameObject.AddComponent<AudioSource>();
            currentSource.outputAudioMixerGroup = uiGroup;
            print("WE HAD TO CREATE A NEW SOURCE!");
        }

        currentSource.clip = clips[id];
        
        currentSource.volume = 1f;
        if(id == 0 || id == 1) currentSource.volume = 0.1f;
        if(id == 2 || id == 3) currentSource.volume = 0.25f;
        if(id == 4 || id == 5) currentSource.volume = MusicManager.instance.volume;

        currentSource.Play();

        StartCoroutine(ReturnToPool(currentSource));
    }

    IEnumerator ReturnToPool(AudioSource source){
        yield return new WaitForSecondsRealtime(source.clip.length + 0.1f);
        
        source.Stop();
        pooledSources.Add(source);
    }
}
