using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using System.Linq;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class NetworkedGameManager : NetworkBehaviour
{

    #region Singleton

    public static NetworkedGameManager instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instaces of NetworkedGameManager found. Deleting this gameobject");
            Destroy(gameObject);
        }
    }

    #endregion


    NetworkVariable<bool> GameHasStarted = new NetworkVariable<bool>(){
        Value = false
    };


    //This is generic for anything we need to keep track of 
    List<ulong> clientsWithTask;

    int numOfClientsWithTask;

    ClientRpcParams clientRpc = new ClientRpcParams();

    Scene loadedScene;
    bool sceneHasLoaded;


    void Awake() {
        SetSingleton();
    }
    public void SetGameStarted(bool v){
        if(!IsServer) {
            Debug.LogWarning("Trying to set game start state without being a host");
            return;
        }

        GameHasStarted.Value = v;
    }
    public bool HasGameStarted(){
        return GameHasStarted.Value;
    }

    #region Start Setup Stuff (Disable Player Movement and whatnot)

    public async Task PrepareGameStart(){
        //Only run on the Server
        if(!IsServer) return;

        //We reset the Tracking variables
        clientsWithTask = new List<ulong>();
        numOfClientsWithTask = 0;
        
        //Just tell all the players to disable movement and whatnot
        foreach (var item in NetworkManager.ConnectedClientsIds)
        {
            clientRpc.Send.TargetClientIds = new List<ulong>(){
                NetworkPlayerObject.playerObjectDictionary[item].OwnerClientId
            };

            NetworkPlayerObject.playerObjectDictionary[item].StartGameDisableMovmentClientRpc(clientRpc);
        }

        //Now we wait until everyone has finished or just until 20 seconds have passed
        float timer = 20f;
        while(numOfClientsWithTask < NetworkManager.ConnectedClients.Count && timer > 0f){
            await Task.Yield();
            timer -= Time.deltaTime;
        }
        print("Timer is: " + timer + "  And num of Clients is: " + numOfClientsWithTask);
    }

    #endregion

    #region Set Team Stuff

    public async Task SetAllTeams_HideAndSeek()
    {
        //Only run on the Server
        if(!IsServer) return;

        //We reset the Tracking variables
        clientsWithTask = new List<ulong>();
        numOfClientsWithTask = 0;

        //Calculate 10%-20% rounded up and get a variable to count how many seekers we have already set
        int numberOfSeekersSet = Mathf.CeilToInt(NetworkManager.ConnectedClientsIds.Count * 0.2f);
        int currentNumberOfSeekers = 0;
        print(numberOfSeekersSet);

        //Shuffle the players so we assign them randomly
        ulong[] shuffledPlayers = shufflePlayers(NetworkManager.ConnectedClientsIds.ToArray());

        //Set the player to a Seeker if the "Seeker Quota" hasn't been met. If it has, set them to a Hider
        foreach (var playerID in shuffledPlayers)
        {
            if(currentNumberOfSeekers < numberOfSeekersSet)
            {
                NetworkPlayerObject.playerObjectDictionary[playerID].ServerSetTeam(2);
                currentNumberOfSeekers++;
            }else{
                NetworkPlayerObject.playerObjectDictionary[playerID].ServerSetTeam(1);
            }
        }

        float timer = 20f;
        while(numOfClientsWithTask < NetworkManager.ConnectedClients.Count && timer > 0f){
            await Task.Yield();
            timer -= Time.deltaTime;
        }

        print("Teams are done with setup, we can start the game!");
    }

    public async Task ResetPlayerTeams(){
        //Only run on the Server
        if(!IsServer) return;

        //We reset the Tracking variables
        clientsWithTask = new List<ulong>();
        numOfClientsWithTask = 0;
        

        foreach (var playerID in NetworkManager.ConnectedClientsIds.ToArray())
        {
            NetworkPlayerObject.playerObjectDictionary[playerID].ServerSetTeam(0);
        }

        float timer = 20f;
        while(numOfClientsWithTask < NetworkManager.ConnectedClients.Count && timer > 0f){
            await Task.Yield();
            timer -= Time.deltaTime;
        }
    }

    
    
    //STOLEN CODE FROM: "https://forum.unity.com/threads/randomize-array-in-c.86871/"
    ulong[] shufflePlayers(ulong[] ids)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < ids.Length; t++ )
        {
            ulong tmp = ids[t];
            int r = UnityEngine.Random.Range(t, ids.Length);
            ids[t] = ids[r];
            ids[r] = tmp;
        }
        return ids;
    }

    #endregion

    #region Load/Unload Scene Stuff
    public async Task LoadScenesForAll(){
        //This is only run on the host
        if(!IsHost) return;

        //reset variables
        sceneHasLoaded = false;

        //Get the current amount of players

        //Calculate what map to load

        //load that map

        //First we subscribe to the event so we can store data
        NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        //Load it on every client
        SceneEventProgressStatus progressStatus = NetworkManager.SceneManager.LoadScene(MapSceneManager.instance.GetActiveMap(), LoadSceneMode.Additive);

        while(!sceneHasLoaded) {await Task.Yield();}

        sceneHasLoaded = false;

        //We are done!
    }


    void OnSceneEvent(SceneEvent e){
        //if every client is done then we can go!
        if(e.ClientId == NetworkManager.ServerClientId && !string.IsNullOrEmpty(e.Scene.name)){
            loadedScene = e.Scene;
            print("We set scene: " + e.Scene.name + " to our currently loaded scene: " + loadedScene.name);
        }
        if(e.SceneEventType == SceneEventType.LoadEventCompleted && IsHost){
            sceneHasLoaded = true;
            NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
    }

    public void UnloadGameForAll(){
        //This is only run on the host
        if(!IsHost) return;

        //unload the active scene
        print("We are unloading! Scene: " + loadedScene.name);
        NetworkManager.SceneManager.UnloadScene(loadedScene);

        //We are done! (THIS ISN'T DONE ON EVERY CLIENT BY THE TIME THIS IS DONE)
    }
    #endregion

    #region Teleport Players


    //THIS NEEDS TO CHANGE - we need options for maps
    public async Task TeleportAllPlayers(bool toLobby){
        //We reset the tracking variables
        clientsWithTask = new List<ulong>();
        numOfClientsWithTask = 0;
        
        foreach (var id in NetworkManager.ConnectedClientsIds)
        {
            NetworkPlayerObject.playerObjectDictionary[id].ServerTeleport(toLobby);
        }

        float timer = 20f;
        while(numOfClientsWithTask < NetworkManager.ConnectedClients.Count && timer > 0f){
            await Task.Yield();
            timer -= Time.deltaTime;
        }
    }

    #endregion

    #region Set Timer Stuff
    
    public void SetTimerForClients(){
        //This is one of the only functions that doesn't track player progress
        
        //This is sent to every client
        SetTimerClientRpc(SyncedTimer.instance.time + 6f, GameManager.instance.maxTimer);
    }

    [ClientRpc]
    void SetTimerClientRpc(float timeToStart, float time){
        //Also using this for the music
        MusicManager.instance.SetMusic(1);
        print("We have set the music!");

        print("setting time at: " + time + "  With delay: " + (timeToStart - SyncedTimer.instance.time));
        Timer.instance.SetTimerWithDelay(time, timeToStart - SyncedTimer.instance.time);
    }

    #endregion

    #region Finish Game Setup

    public void FinishGameSetup(){
        //This just tells all the clients that the game is done with it's setup
        GameDoneSetupClientRpc();
    }

    [ClientRpc]
    void GameDoneSetupClientRpc(){
        Player.isRightPlayer.StartGame();
    }

    #endregion
    
    #region End Game Stuff
    
    /// <summary>
    /// This tells all the clients that we are ending the game
    /// </summary>
    /// <returns></returns>
    public async Task EndGameServer(){
        //Only run on the Server
        if(!IsServer) return;

        //We reset the Tracking variables
        clientsWithTask = new List<ulong>();
        numOfClientsWithTask = 0;

        //Just tell all the players to disable movement and whatnot
        foreach (var item in NetworkManager.ConnectedClientsIds)
        {
            clientRpc.Send.TargetClientIds = new List<ulong>(){
                NetworkPlayerObject.playerObjectDictionary[item].OwnerClientId
            };

            NetworkPlayerObject.playerObjectDictionary[item].EndGameDisableMovementClientRpc(clientRpc);
        }

        //Now we wait until everyone has finished or just until 20 seconds have passed
        float timer = 20f;
        while(numOfClientsWithTask < NetworkManager.ConnectedClients.Count && timer > 0f){
            await Task.Yield();
            timer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// This tells all the clients that we are done going back to the lobby and we can start stuff
    /// </summary>
    public void EndGameDoneServer(){
        //Only run on the Server
        if(!IsServer) return;

        //This task is also not confirmed

        foreach(var item in NetworkManager.ConnectedClientsIds){
            clientRpc.Send.TargetClientIds = new List<ulong>(){
                NetworkPlayerObject.playerObjectDictionary[item].OwnerClientId
            };

            NetworkPlayerObject.playerObjectDictionary[item].EndGameEnableMovementClientRpc(clientRpc);
        }
    }

    

    #endregion
    
    public void ConfirmTask(ulong id){
        //This just makes sure the same client can't confirm twice
        if(clientsWithTask.Contains(id)) return;

        clientsWithTask.Add(id);
        numOfClientsWithTask++;
    }
    //We also want to receive data like Time and whatnot

    #region Leave Game Stuff

    public async void Disconnect(){
        if(!IsHost) {
            NetworkPlayerObject.Singleton.ResetDictionary();
            PlayerNickname.ResetData();
            NetworkManager.Shutdown();
        }
        else{
            print("Made it here 1");
            ClientDisconnectClientRpc();

            while(NetworkManager.ConnectedClientsIds.Count > 1){
                await Task.Yield();
            }

            NetworkPlayerObject.Singleton.ResetDictionary();
            PlayerNickname.ResetData();
            NetworkManager.Shutdown();
        }
    }

    

    [ClientRpc]
    void ClientDisconnectClientRpc(){
        if(IsHost) return;
        print("Made it here 2");
        NetworkPlayerObject.Singleton.ResetDictionary();
        PlayerNickname.ResetData();

        NetworkManager.Shutdown();
        SceneManager.LoadScene(0);
    }

    #endregion
}
