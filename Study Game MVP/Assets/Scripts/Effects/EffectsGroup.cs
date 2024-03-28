using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsGroup : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] AudioSource[] audioSources;

    void Start(){
        if(particleSystems.Length == 0){
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        if(audioSources.Length == 0){
            audioSources = GetComponentsInChildren<AudioSource>();
        }
    }

    public void StartEffect(){
        foreach (var item in particleSystems)
        {
            if(item.isPlaying){
                item.Stop();
                item.Clear();
            }
            item.Play();
        }

        foreach (var item in audioSources)
        {
            item.Play();
        }
    }

    public void StopEffect(bool destroyParticles){
        foreach (var item in particleSystems)
        {
            if(destroyParticles) item.Clear();
            item.Stop();
        }

        foreach (var item in audioSources)
        {
            item.Stop();
        }
    }
}
