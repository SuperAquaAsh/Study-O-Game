using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using TMPro;

public class MapSceneManager : MonoBehaviour
{
    #region Singleton
    public static MapSceneManager instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of MapSceneManager found! Deleting this gameobject");
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    [SerializeField] string[] gameMaps;

    [SerializeField] TextMeshProUGUI mapName;

    string currentMapName;
    int currentMapID;

    void Awake(){
        SetSingleton();
        if(gameMaps.Length > 0) {
            currentMapID = 0;
            currentMapName = gameMaps[currentMapID];
        }
    }

    void Start(){
        if(currentMapName != null){
            UpdateUI();
        }
    }
    public string GetActiveMap(){
        return currentMapName;
    }

    public void SetMap(int id){
        if(gameMaps.Length > id) {
            currentMapID = id;
            currentMapName = gameMaps[currentMapID];
        }
        else{
            Debug.LogWarning("Setting map Id too high!");
            currentMapID = gameMaps.Length - 1;
            currentMapName = gameMaps[currentMapID];
        }
    }

    public void ChangeMap(int v){
        currentMapID += v;
        while(currentMapID >= gameMaps.Length){
            currentMapID -= gameMaps.Length;
        }
        if(currentMapID < 0){
            currentMapID = gameMaps.Length;
        }
        currentMapID = Mathf.Clamp(currentMapID, 0, gameMaps.Length - 1);
        currentMapName = gameMaps[currentMapID];

        UpdateUI();
    }

    void UpdateUI(){
        if(mapName != null) mapName.text = currentMapName;
    }
}
