using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class SyncedTimer : NetworkBehaviour
{
    #region Singleton
    public static SyncedTimer instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instaces of SyncedTimer found, deleting gameobject");
            Destroy(gameObject);
        }
    }

    #endregion
    public float time {get; private set;}
    bool readyForCount;

    private void Awake() {
        SetSingleton();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SyncTimerWebGL());
    }

    // Update is called once per frame
    void Update()
    {
        if(readyForCount) time += Time.deltaTime;
    }

    async void SyncTimer(){
        int numOfSyncs = 0;
        float guessedTime;

        float beforeTime;

        //We wait until the timer isn't zero
        while((NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat) == 0) await Task.Delay(50);
        

        time = (NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat)/2;

        for (int i = 0; i < 100; i++){
            //This is if we stop the game mid-task
            if(NetworkManager.Singleton == null) break;
            //First, we request the time
            guessedTime = (NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat)/2;
            //We increase the amount of requests
            numOfSyncs++;

            //We then average out that time with what we have
            time = ((time * (numOfSyncs - 1)) + guessedTime) / numOfSyncs;

            beforeTime = Time.time;
            //Now we wait a bit to get different results
            await Task.Delay(50);
            time += Time.time - beforeTime;
        }
        readyForCount = true;
    }

    IEnumerator SyncTimerWebGL(){
        int numOfSyncs = 0;
        float guessedTime;

        float beforeTime;

        //We wait until the timer isn't zero
        while((NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat) == 0) yield return null;
        

        time = (NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat)/2;

        for (int i = 0; i < 100; i++){
            //This is if we stop the game mid-task
            if(NetworkManager.Singleton == null) break;
            //First, we request the time
            guessedTime = (NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat)/2;
            //We increase the amount of requests
            numOfSyncs++;

            //We then average out that time with what we have
            time = ((time * (numOfSyncs - 1)) + guessedTime) / numOfSyncs;

            beforeTime = Time.time;
            //Now we wait a bit to get different results
            yield return null;
            time += Time.time - beforeTime;
        }
        readyForCount = true;
    }
}
