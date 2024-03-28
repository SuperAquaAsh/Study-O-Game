using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;
using TMPro;

#region Leaderboard Struct

public struct LeaderboardEntry{
    public string playerName;
    public ulong playerId;
    public uint playerPoints;
}


public struct SentLeadEntry : INetworkSerializable{
    public ulong playerId;
    public uint playerPoints;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerPoints);
    }
}

#endregion



/// <summary>
/// This script is VERY IMPORTANT. The server keeps track of stuff to ensure that points are given out fairly
/// This is also mostly server-side. But it does keep track of points locally, but they aren't trusted for
/// the final leaderboard
/// </summary>
public class PointsManager : NetworkBehaviour
{
    #region Singleton
    public static PointsManager instance;
    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two Instances of PointsManager found");
            Destroy(gameObject);
        }
    }
    #endregion

    void Awake(){
        SetSingleton();
    }
    
    //HAVE A LIST OF VALUES HERE TO KEEP TRACK OF HOW MANY POINTS ARE EARNED FOR EACH ACTION (different for each gamemode)
    #region Point Values
    
    #region Hide and Seek Values
    const uint hiderAnswerQuestionValue = 10;
    const uint seekerAnswerQuestionValue = 7;
    const uint seekerTagValue = 20;
    const uint hiderDodgeValue = 40;
    #endregion

    #region Gamemode 2 Values

    #endregion

    #region Gamemode 3 Values

    #endregion

    #endregion

    //HAVE A VARIABLE THAT KEEPS TRACK OF THE POINTS FOR EACH PLAYER (Maybe a dictionary)
    Dictionary<ulong, uint> playerPointsDictionary = new Dictionary<ulong, uint>();

    
    [SerializeField] ScrollRect LeaderboardObj;

    //This is where we will spwan all the leaderboard chlidren
    [SerializeField] Transform leaderboardParentTransform;

    //These are the children that we will spwan to display each players position
    [SerializeField] GameObject leadEnterPrefab;

    [Header("Local Points Management")]
    [SerializeField] TextMeshProUGUI localPointsText;
    [SerializeField] uint localPoints;

    //This sets up
    private void Start() {
        LeaderboardObj.gameObject.SetActive(false);
    }

    //ADD REQUESTS (SERVERRPCS) HERE FOR POINTS
    #region SetPointsRpcs

    public void ResetData(){
        playerPointsDictionary = new Dictionary<ulong, uint>();
    }

    /// <summary>
    /// This basically adds an entry for every player
    /// </summary>
    public void StartData(){
        ulong[] playerIds = NetworkPlayerObject.playerObjectDictionary.Keys.ToArray();
        for (int i = 0; i < playerIds.Length; i++)
        {
            playerPointsDictionary.Add(playerIds[i], 0);
        }
    }

    public void HidePoints(){
        localPoints = 0;
        localPointsText.text = "";
    }

    public void ResetPoints(){
        localPoints = 0;
        localPointsText.text = "0p";
    }


    #region Hide and Seek Values

    /// <summary>
    /// This sets the points on the server
    /// </summary>
    /// <param name="type">0 = Hider answered question
    /// 1 = Seeker answered question
    /// 2 = Seeker tagged someone
    /// 3 = Hider dodged check</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetHASPointsServerRpc(byte type, ServerRpcParams serverRpcParams = default){
        //We set the added value
        uint addedpoints = 0;
        if(type == 0) addedpoints = hiderAnswerQuestionValue;
        else if(type == 1) addedpoints = seekerAnswerQuestionValue;
        else if(type == 2) addedpoints = seekerTagValue;
        else if(type == 3) addedpoints = hiderDodgeValue;
        else Debug.LogWarning("No type associated with action. Please update code. Adding no points");

        //If the player already exisits, we add points, if not we create a new entery and set the points to the added points
        if(playerPointsDictionary.ContainsKey(serverRpcParams.Receive.SenderClientId)) playerPointsDictionary[serverRpcParams.Receive.SenderClientId] += addedpoints;
        else playerPointsDictionary.Add(serverRpcParams.Receive.SenderClientId, addedpoints);        
    }

    public void SetHASPoints(byte type){
        SetHASPointsServerRpc(type);

        //We set the added value
        uint addedpoints = 0;
        if(type == 0) addedpoints = hiderAnswerQuestionValue;
        else if(type == 1) addedpoints = seekerAnswerQuestionValue;
        else if(type == 2) addedpoints = seekerTagValue;
        else if(type == 3) addedpoints = hiderDodgeValue;
        else Debug.LogWarning("No type associated with action. Please update code. Adding no points");

        localPoints += addedpoints;

        localPointsText.text = localPoints + "p";

        //MAYBE, add code for an effect
    }
    #endregion

    #region Gamemode 2 Values

    #endregion

    #region Gamemode 3 Values

    #endregion

    #endregion

    #region Send Data to clients

    public void SendLeaderboardToClients(){
        //Only run by the host
        if(!IsHost) return;

        ReceiveLeaderboardDataClientRpc(DicToSentLead(playerPointsDictionary));
    }
    [ClientRpc]
    void ReceiveLeaderboardDataClientRpc(SentLeadEntry[] sentEntries){
        //Also stop the music
        MusicManager.instance.SetMusic(-1, 1f);
        MusicManager.instance.SetLowPass(false, 1f);

        //Hide the question
        QuestionDisplay.instance.HideQuestion(true);
        
        //Show the leaderboard
        LeaderboardObj.gameObject.SetActive(true);
        
        //SET THE DATA THAT IS SENT
        SpwanLeaderboardItems(sentEntries);

        //Move the scrollbar to the top
        LeaderboardObj.normalizedPosition = new Vector2(0f, 2f);
    }

    void SpwanLeaderboardItems(SentLeadEntry[] sentEntries){
        //We delete all the children
        for (int i = 0; i < leaderboardParentTransform.childCount; i++)
        {
            Destroy(leaderboardParentTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < sentEntries.Length; i++)
        {
            //Lets spwan in the object
            GameObject go = Instantiate(leadEnterPrefab, leaderboardParentTransform);
            go.GetComponent<LeaderboardObject>().SetData(sentEntries[i].playerId, sentEntries[i].playerPoints);
        }
    }

    SentLeadEntry[] DicToSentLead(Dictionary<ulong, uint> entries){
        SentLeadEntry[] sentLeadEntries = new SentLeadEntry[entries.Count];
        ulong[] ids = entries.Keys.ToArray();
        uint[] points = entries.Values.ToArray();
        for (int i = 0; i < sentLeadEntries.Length; i++)
        {
            sentLeadEntries[i].playerId = ids[i];
            sentLeadEntries[i].playerPoints = points[i];
        }

        //Sort array here!
        sentLeadEntries = SortLeadArray(sentLeadEntries);
        return sentLeadEntries;
    }

    //ADD CODE HERE!!!
    SentLeadEntry[] SortLeadArray(SentLeadEntry[] lead){
        SentLeadEntry[] sortedArray = new SentLeadEntry[lead.Length];
        List<SentLeadEntry> enteredLeads = new List<SentLeadEntry>();

        SentLeadEntry highestEntry = new SentLeadEntry();

        for (int i = 0; i < sortedArray.Length; i++)
        {
            //find the highest value that we HAVEN'T ADDED!
            for(int n = 0; n < sortedArray.Length; n++){
                if(lead[n].playerPoints > highestEntry.playerPoints && !enteredLeads.Contains(lead[n])){
                    highestEntry = lead[n];
                }
            }

            //Now with that, we count that we added it and add it to the sorted array
            sortedArray[i] = highestEntry;
            enteredLeads.Add(highestEntry);
            highestEntry = new SentLeadEntry(){
                playerPoints = 0
            };
            
        }

        return sortedArray;
    }

    public void HideLeaderboard(){
        //Show the leaderboard
        LeaderboardObj.gameObject.SetActive(false);
        HidePoints();
    }

    #endregion
}
