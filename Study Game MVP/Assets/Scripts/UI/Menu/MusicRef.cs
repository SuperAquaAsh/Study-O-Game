using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This allows us to always reference the music Manager
/// </summary>
public class MusicRef : MonoBehaviour
{
    public void SetMusic(int v){
        MusicManager.instance.SetMusic(v, 0.5f);
    }
}
