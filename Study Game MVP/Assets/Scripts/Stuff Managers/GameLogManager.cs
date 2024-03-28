using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class GameLogManager : NetworkBehaviour
{
    #region Singleton
    public static GameLogManager instance; 
    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of GameLogManager found! Destroying this gameobject");
            Destroy(gameObject);
        }
    }
    #endregion

    public const int MaxLeadEntries = 5;
    public const int DistBetweenEntries = 25;
    public const float EntryLerpTime = 0.5f;


    [SerializeField] GameObject logEntry;
    [SerializeField] Transform logParent;
    [SerializeField] GameLogBackdrop logBackdrop;
    int initializeSize = MaxLeadEntries + 5;
    int highestIndex = 0;


    List<GameLogEntry> pooledLogs = new List<GameLogEntry>();
    List<GameLogEntry> activeEntries = new List<GameLogEntry>();

    void Awake(){
        SetSingleton();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameLogEntry hasGameLog = null;
        logEntry.TryGetComponent<GameLogEntry>(out hasGameLog);
        if(hasGameLog == null){
            Debug.LogError("The logEntry object attached to the GameLogManager doesn't have a GameLogEntry Component Attached. Please attach a GameLogEntry to the prefab");
        }

        for(int i = 0; i < initializeSize; i++){
            GameObject go = Instantiate(logEntry, logParent);
            pooledLogs.Add(go.GetComponent<GameLogEntry>());
            go.SetActive(false);
        }
    }

    #region Send Entrys Online

    #region Offeder Sends
    /// <summary>
    /// This is ALWAYS sent by the offender (the person who escaped or tagged)
    /// </summary>
    /// <param name="victimId"></param>
    /// <param name="type"></param>
    public void AddEntryOnline(ulong victimId, int type){
        //Spwan it on our end first
        SpwanEntry(Player.isRightPlayer.OwnerClientId, victimId, type);

        //Now tell the server
        RecieveEntryServerRpc(victimId, type);
    }

    [ServerRpc(RequireOwnership = false)]
    void RecieveEntryServerRpc(ulong victimId, int type, ServerRpcParams rpcParams = default){
        //Create a ClientRpcParams that removes the senders ID
        List<ulong> targetIds = NetworkManager.ConnectedClientsIds.ToList();
        targetIds.Remove(rpcParams.Receive.SenderClientId);

        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{
                TargetClientIds = targetIds
            }
        };

        RecieveEntryClientRpc(rpcParams.Receive.SenderClientId, victimId, type, clientRpcParams);
    }

    #endregion

    #region Victim Sends

    public void AddVictimEntryOnline(ulong offenderId, int type){
        //Spwan it on our end first
        SpwanEntry(offenderId, Player.isRightPlayer.OwnerClientId, type);

        //Now tell the server
        RecieveVictimEntryServerRpc(offenderId, type);
    }

    [ServerRpc(RequireOwnership = false)]
    void RecieveVictimEntryServerRpc(ulong offenderId, int type, ServerRpcParams rpcParams = default){
        //Create a ClientRpcParams that removes the senders ID
        List<ulong> targetIds = NetworkManager.ConnectedClientsIds.ToList();
        targetIds.Remove(rpcParams.Receive.SenderClientId);

        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{
                TargetClientIds = targetIds
            }
        };

        print("The server knows that: " + PlayerNickname.playerIdName[offenderId] + " Hit: " + PlayerNickname.playerIdName[rpcParams.Receive.SenderClientId]);

        RecieveEntryClientRpc(offenderId, rpcParams.Receive.SenderClientId, type, clientRpcParams);
    }

    #endregion

    [ClientRpc]
    void RecieveEntryClientRpc(ulong offenderId, ulong victimId, int type, ClientRpcParams rpcParams = default){
        SpwanEntry(offenderId, victimId, type);
    }

    #endregion

    public GameLogEntry SpwanEntry(ulong offenderId, ulong victimId, int type){
        GameLogEntry entry;

        if(pooledLogs.Count > 0){
            entry = pooledLogs[0];
            pooledLogs.Remove(entry);
        }else{
            Debug.LogWarning("Too many entries were spwaned, making a new entry");
            entry = Instantiate(logEntry, logParent).GetComponent<GameLogEntry>();
        }

        foreach(GameLogEntry logEntry in activeEntries){
            logEntry.NextEntry();
        }
        
        string offenderName = PlayerNickname.playerIdName[offenderId];
        string victimName = PlayerNickname.playerIdName[victimId];
        entry.gameObject.SetActive(true);
        entry.SpwanInEntry();
        if(type == 0) entry.SetText(offenderName + " " + entry.RandomTagVerb() + " " + victimName);
        if(type == 1) entry.SetText(offenderName + " " + entry.RandomEscVerb() + " " + victimName);

        activeEntries.Add(entry);

        highestIndex = 0;
        foreach(GameLogEntry logEntry in activeEntries){
            int index = logEntry.GetIndex();
            if(index > highestIndex) highestIndex = index;
        }
        logBackdrop.SetSize(highestIndex);
        
        return entry;
    }

    public void ReturnEntry(GameLogEntry logEntry){
        logEntry.gameObject.SetActive(false);

        pooledLogs.Add(logEntry);

        if(activeEntries.Contains(logEntry)){
            activeEntries.Remove(logEntry);
        }else{
            Debug.LogWarning("Returning objects to pool that weren't active, thats a bit of an issue");
        }

        //Now we recalcuate the next highest entry
        if(activeEntries.Count > 0){
            highestIndex = 0;
            foreach(GameLogEntry entry in activeEntries){
                int index = entry.GetIndex();
                if(index > highestIndex) highestIndex = index;
            }
            logBackdrop.SetSize(highestIndex);
        }else{
            highestIndex = -1;
            logBackdrop.SetSize(highestIndex);
        }
    }
}
