using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Unity.VisualScripting;


//This VERY IMPORTANT script manages the game state
public class GameManager : MonoBehaviour
{

    #region Singleton
    public static GameManager instance {get; private set;}
    private void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of GameManager Found. Please ensure only one isntance exists at a time");
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this);
    }

    #endregion

    #region Variables
    public bool isRelay {get; private set;}
    public bool isHost {get; private set;}

    // 0 = hide and seek
    public byte gamemode {get; private set;} = 0;
    public int maxConnections {get; private set;} = 10;
    public string address {get; private set;} = "";
    public string joinCode {get; private set;} = "";
    public string quizLocation {get; private set;} = "";

    public string quizName {get; private set;} = "";

    public float maxTimer {get; private set;} = 90;

    public bool gameStarting {get; private set;} = false;
    public bool gameStarted {get; private set;}
    #endregion

    #region Extra Variables

    [Header("Setup Menu")]
    bool isSetupMenuUp;
    [SerializeField] GameObject setupMenuGO;
    [SerializeField] GameObject clientSetupMenuGO;
    TextMeshProUGUI joinCodeText;

    #endregion

    void Update() {
        if(Player.isRightPlayer != null) if(Input.GetKeyDown(KeyCode.Escape) && !Player.isRightPlayer.localHiding && !gameStarting) ToggleSetupMenu();
    }


    #region Set Data Functions
    public void SetConnectionType(bool relay){
        isRelay = relay;
    }

    public void SetHost(bool host){
        isHost = host;
    }

    public void SetConnectionData(string data){
        if(isHost){
            Debug.LogWarning("You are trying to connect to someone when you are the host");
            return;
        }
        if(isRelay){
            joinCode = data;
        }else{
            address = data;
        }

        print("Connection data set to: " + data);
    }

    public void SetQuizLocation(string location){
        quizLocation = location;
        print("Set quiz from: " + quizLocation);

        //then we actually set the quiz
        QuizManager.instance.SetQuizFromLocation(location);
    }
    public void SetQuizName(string name){
        quizName = name;
    }
    public void SetConnectionNumber(int num){
        maxConnections = num;
    }

    public void SetTimer(int time){
        maxTimer = time;
    }



    public void SetGameStart(bool v){
        gameStarted = v;
    }

    public void SetGameStarting(bool v){
        gameStarting = v;
    }
    
    #endregion

    #region Get Data Functions
    public string GetConnectionData(){
        if(isRelay) return joinCode;
        else return address;
    }

    #endregion

    #region Initialize Game Functions
    
    public async void InitializeGame(){
        //We set the quiz to garbage so we can track it in the future
        quizLocation = "No";

        //lets fade to scilence (or lobby music when its done)
        MusicManager.instance.SetMusic(-1, 1f);

        //We need to load the required scence (I'm not sure if this acutally works)
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        while(!op.isDone) await Task.Yield();

        //Then Make sure the quiz manager knows what quiz it is working with
        QuizManager.instance.SetQuizFromLocation(quizLocation);

        //Code splits from here
        if(isRelay) await InitializeRelay();
        else InitializePorts();
    }

    #region Relay Initializationg
    //This is run to setup relay, just be ready to change it to return something in a Task
    async Task InitializeRelay(){
        //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData();
        if(isHost) joinCode = await InitializeRelayHost();
        else await InitializeRelayClient(joinCode);

        if(isHost) ToggleJoinCode(true);
    }

    //Code inspired by: https://docs.unity.com/ugs/en-us/manual/authentication/manual/dsa-notifications
    async Task GetNotificationsIfNeeded(){
        //ADD CODE TO CHECK LAST READ DATE AND IF THE DATE IS OKAY!!!
        //ALSO: CHANGE CODE SO THE SIGN IN ONLY CONTINUES IF ITS ALL OKAY

        List<Unity.Services.Authentication.Notification> notifications = null;
        try
        {
            //Get the last read date
            long lastReadDate = SettingsManager.instance.GetNotificationReadDate();

            //Get the created date
            string lastCreateDate = AuthenticationService.Instance.LastNotificationDate;

            //compare them and only GetNotificationsAsync IF the created date is more

            if(lastCreateDate != null && long.Parse(lastCreateDate) > lastReadDate)
            {
                notifications = await AuthenticationService.Instance.GetNotificationsAsync();
            }
        }
        catch (AuthenticationException e)
        {
            // Read notifications from the banned player exception
            notifications = e.Notifications;
            //It will be displayed later
        }
        catch (Exception e)
        {
            NotificationManager.instance.SetNotification(e.Message, "Close");
            Debug.LogError(e);
            return;
        }

        
        if(notifications == null) return;

        //Lets get the most recent notification
        Unity.Services.Authentication.Notification not = new Unity.Services.Authentication.Notification();
        long bestDate = 0;
        foreach(var item in notifications){
            long date = long.Parse(item.CreatedAt);
            if(date > bestDate) {
                bestDate = date;
                not = item;
            }
        }

        if(bestDate != 0){
            NotificationManager.instance.SetNotification(not.Message, "Okay");
            //save best date to disk
            SettingsManager.instance.SetNotificationReadDate(bestDate);
            await SettingsManager.instance.SaveSettings();
        }
    }

    //Code directly from https://docs.unity.com/ugs/en-us/manual/relay/manual/relay-and-ngo
    async Task<string> InitializeRelayHost(){

        try{
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await GetNotificationsIfNeeded();
            }
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "wss"));
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return NetworkManager.Singleton.StartHost() ? joinCode : null;
        }catch(Exception e){
            NotificationManager.instance.OnNotificationClose += ReturnToMenu;
            NotificationManager.instance.SetNotification(e.Message, "Return To Menu", true);
            return null;
        }
    }

    async Task<bool> InitializeRelayClient(string clientJoinCode){
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try{
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: clientJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "wss"));
        }catch(Exception e){
            NotificationManager.instance.OnNotificationClose += ReturnToMenu;
            NotificationManager.instance.SetNotification(e.Message, "Return To Menu", true);
        }
        
        return !string.IsNullOrEmpty(clientJoinCode) && NetworkManager.Singleton.StartClient();
    }
    #endregion

    //Sets up data for the Ports
    void InitializePorts(){
        //HOPEfully... This works...
        if(isHost) address = "127.0.0.1";
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(address, 4852);

        try {
            if(isHost) NetworkManager.Singleton.StartHost();
            else NetworkManager.Singleton.StartClient();
        } catch(Exception e){
            NotificationManager.instance.OnNotificationClose += ReturnToMenu;
            NotificationManager.instance.SetNotification(e.Message, "Return To Menu", true);
        }
    }

    public void ReturnToMenu(object s, EventArgs e){
        //We will also start the right music
        MusicManager.instance.SetMusic(0, 0.5f);

        SceneManager.LoadScene(0);
        NotificationManager.instance.OnNotificationClose -= ReturnToMenu;
    }

    #endregion

    #region Start Game Functions

    public async void StartGame(){
        //We can't start if the quiz is garbage
        if(quizLocation == "No"){
            //ADD: Tell the player in some sense that they messed up
            return;
        }
        gameStarting = true;
        gameStarted = false;
        ToggleSetupMenu();
        ToggleJoinCode(false);

        //First, the server resets the leaderboard
        PointsManager.instance.ResetData();

        //Next, it sets the data right
        PointsManager.instance.StartData();
        
        //Next, we tell everyone we are starting
        await NetworkedGameManager.instance.PrepareGameStart();

        //ADD CODE HERE TO SEND GAMEMODE TO CLIENTS

        //First, we want to ensure that everyone has the quiz
        await QuizManager.instance.ConfirmQuizzes();

        //Next, we load the scene for everyone
        await NetworkedGameManager.instance.LoadScenesForAll();

        //Next, we change the amount of hiding spots
        HidingManager.instance.DisableSpots();

        //Next, we set up the teams (Change for different Gamemodes)
        await SetupTeamsHAS();

        //Next, we want to Make sure everyone teleports
        await NetworkedGameManager.instance.TeleportAllPlayers(false);

        //Tell everyone to start a synced 3 second timer and then the game has begun (And set the music for every client)
        NetworkedGameManager.instance.SetTimerForClients();

        //We are done! Tell all the clients
        NetworkedGameManager.instance.FinishGameSetup();

        //Now we subscribe to ensure that we know when the game ends
        Timer.instance.OnTimerEnd += EndGame;

        
    }

    /// <summary>
    /// Set the gamestarted bool to ensure that the game has started
    /// </summary>

    void ToggleSetupMenu(){
        //Assign an object to the setupMenuGO object
        if(setupMenuGO == null){
            setupMenuGO = MainCanvas.instance.GameSetup;
        }
        if(clientSetupMenuGO == null){
            clientSetupMenuGO = MainCanvas.instance.clientGameSetup;
        }

        //Toggle the setup Menu
        if(isHost) setupMenuGO.SetActive(!isSetupMenuUp);
        else clientSetupMenuGO.SetActive(!isSetupMenuUp);
        isSetupMenuUp = !isSetupMenuUp;

        //if we put the menu up, then we disable player input. And Visa Versa
        Player.isRightPlayer.CanBeInput(!isSetupMenuUp);
    }

    public void SetSetupMenuState(bool v){
        //Assign an object to the setupMenuGO object
        if(setupMenuGO == null){
            setupMenuGO = MainCanvas.instance.GameSetup;
        }
        if(clientSetupMenuGO == null){
            clientSetupMenuGO = MainCanvas.instance.clientGameSetup;
        }

        //Toggle the setup Menu
        if(isHost) setupMenuGO.SetActive(v);
        else clientSetupMenuGO.SetActive(v);
        isSetupMenuUp = v;

        //if we put the menu up, then we disable player input. And Visa Versa
        Player.isRightPlayer.CanBeInput(!isSetupMenuUp);
    }

    public void ToggleJoinCode(bool show){
        //Only do this if we are relay
        if(!isRelay) return;

        if(joinCodeText == null){
            joinCodeText = MainCanvas.instance.joinCodeText;
        }

        joinCodeText.gameObject.SetActive(show);
        joinCodeText.text = "Join Code: " + joinCode;
    }
    
    #region Team Setup

    async Task SetupTeamsHAS(){
        await NetworkedGameManager.instance.SetAllTeams_HideAndSeek();
    }

    #endregion

    #endregion

    #region Runtime Game Functions

    #endregion

    #region End Game Functions

    async void EndGame(object sender, EventArgs e){
        //Only run this if we are the host
        if(!isHost) return;

        //We will unsubscribe so this isn't called until another game starts
        Timer.instance.OnTimerEnd -= EndGame;

        //We tell all the clients the game is over (Unhide and disable movement locally)
        await NetworkedGameManager.instance.EndGameServer();

        //Send the leaderboard to the clients (And fade out the music)
        PointsManager.instance.SendLeaderboardToClients();

        //We wait a bit to let them admire how bad they lost
        await Task.Delay(10000);

        //We change all their teams to NoTeam
        await NetworkedGameManager.instance.ResetPlayerTeams();

        //We teleport all the players back to the lobby
        await NetworkedGameManager.instance.TeleportAllPlayers(true);

        //We unload the game on all clients
        NetworkedGameManager.instance.UnloadGameForAll();

        //We tell all players we are done
        NetworkedGameManager.instance.EndGameDoneServer();

        ToggleJoinCode(true);
    }

    #endregion

    #region Leave Game Functions

    public void LeaveLobby(){
        //if the game isn't starting, we tell the NetworkedGameManager to let us leave
        if(!gameStarting){
            NetworkedGameManager.instance.Disconnect();
            SceneManager.LoadScene(0);
        }
    }

    public void QuitApplication(){
        Application.Quit();
    }

    #endregion

    #region Close Game Functions
    void OnApplicationQuit(){
        print("QUIT!");
        if(NetworkedGameManager.instance != null) LeaveLobby();
    }

    public void OnBrowserClose(){
        print("We are here in the browser console!!!");
        if(NetworkedGameManager.instance != null) LeaveLobby();
    }

    #endregion
}
