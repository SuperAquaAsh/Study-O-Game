using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This sets the spawn points in newly loaded maps
/// </summary>
public class MapSpwans : MonoBehaviour
{
    [SerializeField] SpwanPoint[] GameSpwans;
    // Start is called before the first frame update
    void Start()
    {
        if(GameSpwans.Length == 0) GetAllSpwans();
        
        SpwanManager.instance.SetGameSpwans(GameSpwans);
    }

    void GetAllSpwans(){
        GameSpwans = GetComponentsInChildren<SpwanPoint>();
    }
}
