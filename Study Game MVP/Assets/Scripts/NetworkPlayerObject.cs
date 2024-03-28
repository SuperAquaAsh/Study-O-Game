using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using System;
using Unity.Netcode.Transports.UTP;

//This script allows any player to access the player object of another player just using the ID
public class NetworkPlayerObject : MonoBehaviour
{
    public static NetworkPlayerObject Singleton;
    public static Dictionary<ulong, Player> playerObjectDictionary = new Dictionary<ulong, Player>();

    void Awake(){
        if(Singleton == null){
            Singleton = this;
        }else{
            Debug.LogError("Two instances of 'NetworkPlayerObject' Please remove one");
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Subscribe to know when a client connects
        //NetworkManager.Singleton.OnClientConnectedCallback += ClientConnect;

        //MAYBE: Add any Players that are already Connected to the List
    }
    public void ResetDictionary(){
        playerObjectDictionary.Clear();
    }

    public void ClientConnect(ulong obj)
    {
        //Finds all "Player" scripts and checks for the one that matches the ID of the player that Just joined
        Player[] players = FindObjectsOfType<Player>();
        foreach (var item in players)
        {
            if(item.OwnerClientId == obj)
            {
                playerObjectDictionary.Add(obj, item);
                Debug.Log("Added player with ID: " + obj);
            }
        }
    }
    public void DisconnectClient(ulong id){
        playerObjectDictionary.Remove(id);
        PointersManager.instance.DeletePlayerPointer(id);
    }
}
