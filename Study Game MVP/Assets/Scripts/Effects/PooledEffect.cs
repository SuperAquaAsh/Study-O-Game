using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledEffect : MonoBehaviour
{
    void OnParticleSystemStopped(){
        EffectsManager.ReturnObjectToPool(gameObject);
    }
}
