using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpwanManager : MonoBehaviour
{
    public static SpwanManager instance {get; private set;}
    [SerializeField] SpwanPoint LobbySpwan;

    [SerializeField] SpwanPoint[] GameSpwans;

    void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two Instances of SpwanManger found, deleting gameobject");
            Destroy(gameObject);
        }

        if(transform.position != Vector3.zero){
            transform.position = Vector3.zero;
            Debug.LogWarning("Position of the SpwanManager wasn't centered. Please ensure it's centered for best spwan points");
        }
    }

    public Vector2 GetLobbySpwan(){
        return LobbySpwan.GetSpwanPoint();
    }

    public Vector2 GetGameSpwan(){
        return GameSpwans[Random.Range(0, GameSpwans.Length)].GetSpwanPoint();
    }
    public Vector2 GetGameSpwan(int index){
        return GameSpwans[index].GetSpwanPoint();
    }

    public void SetGameSpwans(SpwanPoint[] spwanPoints){
        GameSpwans = spwanPoints;
    }
}
