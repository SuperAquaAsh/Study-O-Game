using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

public class MusicManager : MonoBehaviour
{
    #region Singleton
    public static MusicManager instance;
    
    void SetSingleton(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Debug.LogWarning("Two instances of MusicManager found! Destroying this gameobject!");
            Destroy(gameObject);
            return;
        }
    }
    #endregion
    
    [SerializeField] AudioSource introSource;
    [SerializeField] AudioSource loopSource;

    [SerializeField] AudioSource fadeSource;

    [SerializeField] AudioSource extraLoopSource;
    [SerializeField] AudioSource extraJuiceLoop;

    [SerializeField] AudioMixer musicMixer;


    [SerializeField] Music[] musics;

    public float volume {get; private set;} = 0.5f;

    float fadeVolume = 1f;

    float lowPass;

    float lowPassLowValue = 250;
    float lowPassHighValue = 22000;

    Coroutine fadeCor;
    Coroutine fadeExtraCor;

    Coroutine lowPassCor;
    Coroutine lowVolCor;

    void Awake(){
        SetSingleton();
        musicMixer.GetFloat("Lowpass", out lowPass);
    }

    void Start(){
        SetMusic(0);
    }

    void Update(){
        if(fadeVolume < 1){
            fadeVolume = 1;
            Debug.LogWarning("fadeVolume was below 1. This increases the volume. Don't set fadeVolume below 1");
        }
        float calculatedVolume = Mathf.Lerp(-80f, 5f, volume / fadeVolume);
        musicMixer.SetFloat("M_Volume", calculatedVolume);
    }

    #region Set Music
    /// <summary>
    /// Sets the music with a fade in (ID of -1 means no music)
    /// </summary>
    /// <param name="v">what id of music should be played (-1 means no music)</param>
    /// <param name="delay">the amount of time it should take to fade in from the current track</param>
    public void SetMusic(int v, float delay = 0f){
        if(v == -1){
            //An ID of -1 means we should fade out
            if(fadeCor != null) StopCoroutine(fadeCor);

            //the musics[0] is just a placeholder, we don't actually fade to it
            fadeCor = StartCoroutine(FadeMusic(ScriptableObject.CreateInstance<Music>(), delay));
        }else if(musics[v] != null && delay > 0f){            
            if(fadeCor != null) StopCoroutine(fadeCor);

            fadeCor = StartCoroutine(FadeMusic(musics[v], delay));
        }else if(musics[v] != null){
            introSource.Pause();
            loopSource.Pause();

            
            //Set the intro
            introSource.clip = musics[v].intro;
            //Set it up so after the intro is done, it will be the loop (Audio is played inside courotine)
            loopSource.playOnAwake = false;
            loopSource.clip = musics[v].loop;
            loopSource.loop = true;

            extraJuiceLoop.playOnAwake = false;
            extraJuiceLoop.clip = musics[v].extraLoop;
            extraJuiceLoop.loop = true;
            extraJuiceLoop.volume = 0;

            introSource.Play();
            float waitTime = introSource.clip.length;

            loopSource.PlayDelayed(waitTime);
            extraJuiceLoop.Play();

            

        }else{
            Debug.LogWarning("Music of ID: " + v + " not found!");
        }
    }

    IEnumerator FadeMusic(Music music, float time){
        fadeSource.Pause();
        extraJuiceLoop.Pause();
        
        bool isSilence = music.loop == null;
        
        //This dictatces if the music we are fading to has an intro or just a loop
        bool loop = music.intro == null;
        bool extraLoop = music.extraLoop != null;
        if(loop && !isSilence) {
            fadeSource.clip = music.loop;
            fadeSource.loop = true;
            fadeSource.playOnAwake = false;
        }
        else if(!isSilence) {
            fadeSource.clip = music.intro;
            fadeSource.loop = false;

            extraLoopSource.clip = music.loop;
            extraLoopSource.loop = true;
            extraLoopSource.playOnAwake = false;
            if(extraLoop){
                extraJuiceLoop.clip = music.extraLoop;
                extraJuiceLoop.loop = true;
                extraJuiceLoop.playOnAwake = false;
                extraJuiceLoop.volume = 0;
            }
        }else{
            fadeSource.clip = null;
            fadeSource.loop = false;
        }

        fadeSource.volume = 0;
        fadeSource.loop = loop;

        float maxTime = time;

        if(!isSilence) fadeSource.Play();
        if(!loop) extraLoopSource.PlayDelayed(fadeSource.clip.length);
        if(!loop && extraLoop) extraJuiceLoop.Play();

        while(time > 0f){
            introSource.volume = Mathf.Lerp(0, 1, time / maxTime);
            loopSource.volume = Mathf.Lerp(0, 1, time / maxTime);

            fadeSource.volume = Mathf.Lerp(1, 0, time / maxTime);
            time -= Time.deltaTime;
            yield return null;
        }

        fadeSource.volume = 1;

        introSource.Pause();
        loopSource.Pause();

        introSource.volume = 1;
        loopSource.volume = 1;

        //Set the clips
        if(!loop) introSource.clip = music.intro;
        if(!isSilence) loopSource.clip = music.loop;

        if(!loop) {
            AudioSource source = introSource;
            introSource = fadeSource;
            fadeSource = source;


            source = loopSource;
            loopSource = extraLoopSource;
            extraLoopSource = source;
        }
        else if(!isSilence) {
            AudioSource source = loopSource;
            loopSource = fadeSource;
            fadeSource = source;

            introSource.volume = 1;
            introSource.Pause();
        }else{
            //This MAY cause errors, remove it if it does
            introSource.volume = 1;
            introSource.clip = null;
            introSource.Pause();

            loopSource.volume = 1;
            loopSource.clip = null;
            loopSource.Pause();
        }
    }

    #endregion

    #region Fade In Extra Loop
    public void SetFadeInExtra(bool fade, float time, float keepTime = -1f){
        if(fadeExtraCor != null) StopCoroutine(fadeExtraCor);

        
        if(keepTime > 0f) fadeExtraCor = StartCoroutine(FadeInExtraAndKeep(fade, time, keepTime));
        else fadeExtraCor = StartCoroutine(FadeInExtra(fade, time));
    }
    IEnumerator FadeInExtra(bool fade, float time){
        
        float start = extraJuiceLoop.volume;
        float goal;
        if(fade) goal = 1;
        else goal = 0;

        if(start == goal) yield break;
        
        float maxTime = time;

        while(time > 0f){
            
            extraJuiceLoop.volume = Mathf.Lerp(goal, start, time / maxTime);

            time -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeInExtraAndKeep(bool fade, float time, float keepTime){
        
        float start = extraJuiceLoop.volume;
        float goal;
        if(fade) goal = 1;
        else goal = 0;

        if(start == goal) yield break;
        
        float maxTime = time;

        while(time > 0f){
            
            extraJuiceLoop.volume = Mathf.Lerp(goal, start, time / maxTime);

            time -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(keepTime);

        time = maxTime;

        while(time > 0f){
            
            extraJuiceLoop.volume = Mathf.Lerp(0, 1, time / maxTime);

            time -= Time.deltaTime;
            yield return null;
        }
    }
    
    #endregion

    #region Low Pass
    public void SetLowPass(bool isOn, float time = 0.5f){
        if(lowPassCor != null) StopCoroutine(lowPassCor);

        lowPassCor = StartCoroutine(FadeLowPass(isOn, time));
    }

    IEnumerator FadeLowPass(bool isOn, float time){
        float start = lowPass;
        float end = lowPassLowValue;
        if(!isOn) end = lowPassHighValue;

        float maxTime = time;

        while(time > 0f){
            lowPass = Mathf.Lerp(end, start, time / maxTime);

            musicMixer.SetFloat("Lowpass", lowPass);

            time -= Time.deltaTime;
            yield return null;
        }

        musicMixer.SetFloat("Lowpass", end);
    }
    #endregion

    #region Set Volume
    public void SetVolume(float v){
        print("volume set to: " + v);
        volume = v;
    }

    public void FadeVolume(float set, float delay){
        if(lowVolCor != null) StopCoroutine(lowVolCor);

        lowVolCor = StartCoroutine(FadeOut(set, delay));
    }

    IEnumerator FadeOut(float set, float time){
        float start = fadeVolume;
        float end = set;

        float maxTime = time;
        while(time > 0f){
            time -= Time.deltaTime;

            fadeVolume = Mathf.Lerp(end, start, time/maxTime);

            yield return null;
        }

        fadeVolume = end;
    }
    #endregion
}