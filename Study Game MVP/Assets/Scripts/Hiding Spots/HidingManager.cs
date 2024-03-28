using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class HidingManager : NetworkBehaviour
{
    #region Singleton
    public static HidingManager instance;
    void SetSingleton(){
        if(instance == null){
            instance = this;
            print("INSTANCE SET FOR: HidingManager");
        }else{
            Debug.LogWarning("Two Instances of HidingManager Found! Destroying this gameobject");
            Destroy(gameObject);
        }
    }
    #endregion
    
    List<Hiding> hidingSpots;

    void Awake(){
        SetSingleton();
    }

    public void SetHidingSpots(List<Hiding> spots){
        if(hidingSpots != null) hidingSpots.Clear();
        hidingSpots = spots;
    }

    public void DisableSpots(){
        //only on the server
        if(!IsHost) return;
        int players = NetworkManager.ConnectedClientsIds.Count();
        int spotsPerPlayer = 3;

        //Shuffle List
        hidingSpots = ShuffleList(hidingSpots);

        int countLeft = players * spotsPerPlayer;
        foreach (var item in hidingSpots)
        {
            if(countLeft > 0) {
                item.SetEnable(true);
                countLeft--;
            }else{
                item.SetEnable(false);
            }
        }
    }
    //STOLEN CODE FROM: "https://forum.unity.com/threads/randomize-array-in-c.86871/"
    List<Hiding> ShuffleList(List<Hiding> v){
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        Hiding[] array = v.ToArray();
        for (int t = 0; t < array.Length; t++ )
        {
            Hiding tmp = array[t];
            int r = UnityEngine.Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
        v = array.ToList();
        return v;
    }
}
