using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;
using System.Linq;
using System.Threading;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class Player : NetworkBehaviour
{
    public GameObject DEBUG_TestEffect;
    [SerializeField] float Speed = 1f;
    
    //The Rigidbody is only simulated on the Owners Client
    [SerializeField] Rigidbody2D rb;
    public Collider2D playerCollider {get; private set;}

    //This exists so I can disable the interpolation for teleporting
    [SerializeField] ClientNetworkTransform netTransform;
    [SerializeField] PlayerNickname playerNickname;
    [SerializeField] PlayerTimer playerTimer;
    public PlayerEffects playerEffects {get; private set;}

    //This is public so the PlayerMovement can access it
    public Animator animator;
    //This is also public so the PlayerMovement can access it
    public PlayerSprites sprites {get; private set;}
    public PlayerMovement movement;
    public float playerSize = 1f;
    public const float maxTimerInSpot = 25f;
    


    [HideInInspector] public NetworkVariable<bool> isHiding;

    //THIS IS IMPORTANT! It restricts every input from the player if false
    bool canPlay = true;

    //Also used to check hiding spots
    Hiding hidingGoal = null;
    public bool localHiding {get; private set;} = false;
    //Like isHiding, but includes when you are pending a request to get in a spot;

    bool PendingUnhide = false;
    //This is used to make sure clients can't rapidly unhide and hide

    [HideInInspector] public Vector2 lastHitDir;
    [HideInInspector] public bool isPendingCheck {get; private set;} = false;
    bool canHide = true;
    bool canUnhide = true;
    float hideTimer = 0f;
    float timerDecay = 0f;

    #region "Team States"
    Team currentTeamState;
    Team_Seeker seeker = new Team_Seeker();
    Team_Hider hider = new Team_Hider();
    Team_NoTeam noTeam = new Team_NoTeam();

    [HideInInspector]
    public enum TeamsEnum{
        NoTeam,
        Hider,
        Seeker,
    }
    #endregion

    //DEBUG! This allows any object to access the player
    public static Player isRightPlayer;

    public NetworkVariable<TeamsEnum> currentTeamEnum {get; private set;} = new NetworkVariable<TeamsEnum>();

    //This is so we can target one player
    ClientRpcParams clientRpc = new ClientRpcParams();
    Player checkingPlayer;

    #region Initialize stuff
    void Start()
    {
        //Assign Variables
        if(rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if(rb == null)
            Debug.LogWarning("No Rigidbody Attached to Player");

        if(playerCollider == null)
            playerCollider = GetComponent<Collider2D>();
        
        if(playerCollider == null)
            Debug.LogWarning("No Collider Attached to Player");

        if(sprites == null)
            sprites = GetComponentInChildren<PlayerSprites>();
        
        if(sprites == null)
            Debug.LogWarning("No PlayerSprites on the Players Children");
        
        if(playerTimer == null)
            playerTimer = GetComponent<PlayerTimer>();

        if(playerTimer == null)
            Debug.LogWarning("No PlayerTimer on the Player");
        if(playerEffects == null)
            playerEffects = GetComponent<PlayerEffects>();

        if(playerEffects == null)
            Debug.LogWarning("No PlayerEffects on the Player");


        //All code above here is done on every client every time a player spwans in
        if(!IsOwner)
        {
            //rb.bodyType = false;
            return;
        }
        //All the code below here is done only on the owner of this player

        //We set ourselves to the right position
        TeleportPlayer(true);

        isRightPlayer = this;
        movement.SetPlayer(this);
        movement.SetRigidbody(rb);

        playerTimer.SetMaxValue(maxTimerInSpot);

        //set the Camera to follow this player
        Camera.main.GetComponent<Follow>().followObject = gameObject;
    }
    public override void OnNetworkSpawn()
    {
        //Set Network Variables in here
        isHiding.OnValueChanged += isHidingChange;
        currentTeamEnum.OnValueChanged += OnTeamChange;

        //Add me to the list of NetworkPlayerObjects
        NetworkPlayerObject.Singleton.ClientConnect(OwnerClientId);

        if(isHiding.Value && !IsOwner)
        {
            sprites.ChangeSpriteVisibility(false);
            rb.simulated = false;
            playerCollider.enabled = false;
        }

        //Team should be NoTeam by default, that is if it is the first enum value listed off
        print(currentTeamEnum.Value);

        if(IsOwner) NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;

        //Set teams
        SetTeam(0, true);

        //We update the nametags when we are ready
        playerNickname.HideNameIfOnTeamsAll(currentTeamState.invisibleTeams);

        //Now we spwan in the Pointer for this player
        PointersManager.instance.SpwanPointer(this);

        //Network Animator set

        base.OnNetworkSpawn();
    }

    #region Initialize Game (Start the game stuff)

    [ClientRpc]
    public void StartGameDisableMovmentClientRpc(ClientRpcParams clientRpc = default){
        if(!IsOwner) return;

        GameManager.instance.SetGameStarting(true);
        GameManager.instance.SetGameStart(false);

        GameManager.instance.SetSetupMenuState(false);

        CanBeInput(false);
        ConfirmStartGameDisableMovmenServerRpc();
        
        //ADD CODE HERE FOR UI
    }
    [ServerRpc]
    void ConfirmStartGameDisableMovmenServerRpc(ServerRpcParams serverRpcParams = default){
        NetworkedGameManager.instance.ConfirmTask(serverRpcParams.Receive.SenderClientId);
    }

    //THIS IS VERY IMPORTANT!!!!
    public void StartGame(ClientRpcParams clientRpc = default){
        //This is run when EVERYONE is done and EVERYTHING is setup, just need to start the movement
        if(!IsOwner) return;

        GameManager.instance.SetGameStart(true);

        //Just start the points at 0
        PointsManager.instance.ResetPoints();

        //ADD CODE HERE FOR UI

        //All code here is for enableing movement
        if(currentTeamEnum.Value == TeamsEnum.Seeker){
            CanBeInputWithDelay(true, Timer.instance.timeUntilCountdown);
            return;
        }

        CanBeInput(true);
    }

    #endregion

    #endregion

    #region End Game

    [ClientRpc]
    public void EndGameDisableMovementClientRpc(ClientRpcParams clientRpc = default){
        if(!IsOwner) return;
        
        //Lets set variables on the game manager
        GameManager.instance.SetGameStarting(false);
        GameManager.instance.SetGameStart(false);

        //Lets change any weird variables
        movement.ChangeState(0, true);

        //Lets hide the timer if it is visable
        playerTimer.ToggleVisuals(false);

        CanBeInput(false);
        ConfirmEndGameDisableMovmenServerRpc();
    }

    [ServerRpc]
    void ConfirmEndGameDisableMovmenServerRpc(ServerRpcParams serverRpcParams = default){
        NetworkedGameManager.instance.ConfirmTask(serverRpcParams.Receive.SenderClientId);
    }



    [ClientRpc]
    public void EndGameEnableMovementClientRpc(ClientRpcParams clientRpc = default){
        if(!IsOwner) return;

        //We allow movement and reset it
        CanBeInput(true);
        movement.ChangeState(0, true);

        //We also remove the leaderboard
        PointsManager.instance.HideLeaderboard();

        //Lets hide the timer if it is visable (we do this twice, but it's fine!)
        playerTimer.ToggleVisuals(false);

        ConfirmEndGameEnableMovementServerRpc();
    }

    [ServerRpc]
    void ConfirmEndGameEnableMovementServerRpc(ServerRpcParams serverRpcParams = default){
        NetworkedGameManager.instance.ConfirmTask(serverRpcParams.Receive.SenderClientId);
    }

    #endregion
    // Update is called once per frame
    void Update()
    {        
        //All code above here is done on every client every time a player spwans in
        if(!IsOwner)
            return;
        //All the code below here is done only on the owner of this player

        if(!canPlay) return;
        //All code below here will only run if the player can play

        movement.OwnerUpdate();

        if(currentTeamState != null) currentTeamState.UpdateTeam(this);
        
        //print(localHiding && !isHiding.Value); This code is for testing if the player is pending

        //Add code here to change Pending Unhide when it will work
    }
    
    
    #region PlayerMovement
    void FixedUpdate(){

        //All code above here is done on every client every time a player spwans in
        if(!IsOwner)
            return;
        //All the code below here is done only on the owner of this player

        if(!canPlay) return;

        movement.OwnerFixedUpdate();

        if(currentTeamState != null) currentTeamState.FixedUpdateTeam(this);
    }
    public void FixedMoveWithSpeed(float speed){
        //Pass this onto the movment handler
        movement.FixedMoveWithSpeed(speed);
    }
    public void ChangeMovementState(int id, bool overrideCurrentState = false){
        movement.ChangeState(id, overrideCurrentState);
    }
    #endregion

    #region Hiding (includes hidden hit stuff)

    #region Actual hiding
    public void hidingInputs(){
        //Only bother if you can hide and unhide
        if(!canHide){
            return;
        }

        //If they are in a hiding spot and press F they get out
        if(localHiding){
            if(Input.GetKeyDown(KeyCode.F) && Application.isFocused && canUnhide)
            {
                //If a conformation from the server hasn't reached the client then you are currently Pending an Unhide
                FullyUnhide();
            }

            return;
        }
        //Any code bellow here will be run when the player isn't hiding

        //Checks for interactalbes, but not if not pending to get in a hiding spot
        if(Input.GetKeyDown(KeyCode.F) && Application.isFocused && !isHiding.Value && !PendingUnhide)
        {
            print("attempting Hide");
            CheckInteract();
        }
    }

    public void FullyUnhide()
    {
        //This is run at almost any instance to unhide
        if(!isHiding.Value){
            PendingUnhide = true;
        }
        LocalUnhide();
        ChangeHidingServerRpc(false, PendingUnhide);

        //Lets unsubscribe for when the question gets answered
        QuestionDisplay.instance.OnAnswer -= HidingAnswer;
        //Hide any displayed questions
        QuestionDisplay.instance.HideQuestion();
    }

    public void SetGoalHidingSpot(Hiding hiding){
        
        hidingGoal = hiding;
        localHiding = true;
        canUnhide = false;

        sprites.ChangeSpriteVisibility(false);
        playerTimer.ToggleVisuals(false);
        playerCollider.enabled = false;
        animator.SetFloat("M_Speed", 0f);

        if(!IsOwner) return;

        MusicManager.instance.SetLowPass(true, 0.4f);
        MusicManager.instance.FadeVolume(1.1f, 0.4f);
        

        currentTeamState.WhenHide(this);
        rb.simulated = false;
        
    }

    [ClientRpc]
    public void FailedHidingClientRpc(){
        //run if attempted to hide, but didn't make it, which includes trying to hide in a spot where this client knows that their is someone is in it
        localHiding = false;
        
        //Sent to every client, so this is run on clients that don't own this object
        sprites.ChangeSpriteVisibility(true);
        playerCollider.enabled = true;

        if(!IsOwner) return;

        rb.simulated = true;
    }
    public void FailedHidingServerSide(){
        isHiding.Value = false;
    }

    void HitFromHidingSpot()
    {
        if(!IsOwner) return;
        print("we are about to be tagged!");
        movement.SetHitDir((transform.position - hidingGoal.transform.position).normalized);
        currentTeamState.WhenTagged(this, false);
        return;
    }
    public void HiddenHit(bool teamChanger){
        //if(!IsOwner) return;

        //This is run on the client that was hit on the player that was hit because they answered a question too slow, code should be added for effects
        FullyUnhide();

        movement.SetHitDir((transform.position - hidingGoal.transform.position).normalized);
        currentTeamState.WhenTagged(this, teamChanger);

        if(!teamChanger) QuestionDisplay.instance.HideQuestion(true);
    }

    [ClientRpc]
    public void HideClientRpc()
    {
        //Sent to ALL clients whenever this player successfully hides
        if(IsOwner && !localHiding){
            FullyUnhide();
        }
        if(IsOwner && localHiding){
            //If I successfully hid, then lets have questions pop up to answer
            QuestionDisplay.instance.DisplayQuestion();

            //Lets subscribe for when the question gets answered
            QuestionDisplay.instance.OnAnswer += HidingAnswer;
        }


        //If I have already locally unhid, don't bother hiding me again
        if(!localHiding && IsOwner) return;

        //Sent to every client, so this is run on clients that don't own this object
        sprites.ChangeSpriteVisibility(false);
        playerCollider.enabled = false;
        animator.SetFloat("M_Speed", 0f);

        if(!IsOwner) return;

        rb.simulated = false;
    }
    void HidingAnswer(object sender, QuestionDisplay.OnAnswerArgs e)
    {
        if(e.isRight){
            //We will add points to us
            if(GameManager.instance.gamemode == 0) PointsManager.instance.SetHASPoints(0);

            ChangeTimer(10f);
            //We add an extra 0.05 seconds to the decay for every second faster than 7 sec they answer (0.45 max)
            if(e.time > 7f) ChangeDecay(0.1f);
            else ChangeDecay(0.1f + ((-e.time + 7) * 0.05f));
            canUnhide = true;
        }
        QuestionDisplay.instance.DisplayQuestion();
    }

    public void isHidingChange(bool previous, bool current)
    {
        //If it wants to hide and I have already locally unhid, don't bother hiding me again
        if(!localHiding && IsOwner) 
        {
            return;
        }

        sprites.ChangeSpriteVisibility(!current);
        rb.simulated = !current;
        playerCollider.enabled = !current;
        if(current) animator.SetFloat("M_Speed", 0f);

        if(!IsOwner) return;

        localHiding = current;
        //Only run this if this isn't the owner, becuase the owner handles it localy when they change the vari
    }

    public void LocalUnhide(){
        animator.SetFloat("M_Speed", 0f);
        sprites.ChangeSpriteVisibility(true);

        playerTimer.SetTimer(hideTimer);
        playerTimer.ToggleVisuals(true);

        MusicManager.instance.SetLowPass(false, 1f);
        MusicManager.instance.FadeVolume(1f, 1f);

        rb.simulated = true;
        playerCollider.enabled = true;
        localHiding = false;
    }
    public void ChangeHidingServerSide(bool value){
        isHiding.Value = value;
    }

    [ClientRpc]
    void ChangePendingClientRpc(){
        print("I am Free!");
        PendingUnhide = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHidingServerRpc(bool value, bool pending){
        if(!value && hidingGoal != null) hidingGoal.Unhide();
        isHiding.Value = value;

        //If the Player is pending an Unhide, set it false on the client
        if(pending) ChangePendingClientRpc();
    }
    #endregion

    #region Hiding Timer
    public void HideTimerUpdate(){
        if(isHiding.Value && !isPendingCheck){
            print("can see: " + QuestionDisplay.instance.canSeeAnswer + " || Showing Wrong: " + QuestionDisplay.instance.showingWrong);
            if(QuestionDisplay.instance.canSeeAnswer || QuestionDisplay.instance.showingWrong) hideTimer -= Time.unscaledDeltaTime + (timerDecay * Time.unscaledDeltaTime);
            //timerDecay += 0.04f * Time.unscaledDeltaTime;
            QuestionDisplay.instance.ChangeSlider(hideTimer);
            if(hideTimer < 0){
                HiddenHit(false);
                QuestionDisplay.instance.DisplaySider(false);
            }
        }
        if(isPendingCheck && QuestionDisplay.instance.SliderActive){
            QuestionDisplay.instance.DisplaySider(false);
        }
        if(!isHiding.Value){
            hideTimer += Time.unscaledDeltaTime * 2;
            hideTimer = Mathf.Clamp(hideTimer, 5f, maxTimerInSpot);

            timerDecay -= Time.deltaTime * 0.5f;
            timerDecay = Mathf.Clamp(timerDecay, -0.5f, 1f);

            playerTimer.SetTimer(hideTimer);
        }
    }
    public void SetTimer(float time){
        hideTimer = time;
        timerDecay = -0.5f;
    }
    public void ChangeTimer(float change){
        hideTimer = Mathf.Clamp(hideTimer + change, float.NegativeInfinity, maxTimerInSpot);
    }
    //This primarrally exists so as the player answers questions, the timer gets quicker
    public void ChangeDecay(float change){
        timerDecay = Mathf.Clamp(timerDecay + change, float.NegativeInfinity, 8f);
        //timerDecay = timerDecay + change;
    }
    public void ToggleHideTimer(bool v){
        playerTimer.ToggleVisuals(v);
    }
    /*
    if(isHiding.Value && NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].IsOwner){
            if(isBeingChecked.Value)
            {
                QuestionDisplay.instance.DisplaySider(false);
                //THIS RETURN MIGHT BE AN ISSUE!
                return;
            }
            timer -= Time.deltaTime + timerDecay;
            timerDecay += Time.deltaTime * 0.00001f;
            QuestionDisplay.instance.ChangeSlider(timer);
            if(timer < 0){
                NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].HiddenHit(false);
                QuestionDisplay.instance.DisplaySider(false);
            }
        }else if(QuestionDisplay.instance.SliderActive) QuestionDisplay.instance.DisplaySider(false);
    */
    #endregion

    #endregion

    #region Spot Checks (Both for hider and checker)
    //This code checks hiding spots around you for you to inspect
    public bool PendHidingSpotCheck(){
        //Check for Hiding spots in front (around) of us
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var item in colliders)
        {
            Hiding spot = item.GetComponent<Hiding>();
            if(spot != null)
            {
                //If we find a spot just check if it isn't being checked by another player
                bool successfullCheck = spot.CheckPendingHidingSpot(OwnerClientId);
                isPendingCheck = successfullCheck;
                if(successfullCheck) {
                    hidingGoal = spot;
                    rb.velocity = Vector2.zero;
                    animator.SetFloat("M_Speed", 0f);
                }
                return successfullCheck;
            }
        }
        return false;
    }

    [ClientRpc]
    public void FailedSpotCheckClientRpc(ClientRpcParams clientRpcParams){
        if(!IsOwner) return;
        //This code is run when someone was faster than you at checking the spot. It can happen even after you answer the question
        isPendingCheck = false;
        //remove the UI for the question
        QuestionDisplay.instance.HideQuestion();

    }
    [ClientRpc]
    public void SuccessfullSpotCheckClientRpc(int questionID){
        if(!IsOwner) return;
        //This code is run when the Server says: "Yes, you can check this spot, and if their is a player in here, the same question number was sent to them"
        isPendingCheck = true;

        //We don't know who we are checking
        checkingPlayer = null;

        //Turn it down, let them focus (the music)
        MusicManager.instance.SetLowPass(true, 0.4f);
        MusicManager.instance.FadeVolume(1.1f, 0.4f);

        //Have the question pop up and subscribe for an answer
        QuestionDisplay.instance.OnAnswer += PendCheckAnswer;
        QuestionDisplay.instance.DisplayQuestion(questionID);

        //Hide the slider for answers
        QuestionDisplay.instance.DisplaySider(false);

        print("I have question: " + questionID);
    }
    [ClientRpc]
    public void BeingCheckedClientRpc(int questionID, ulong checkerId)
    {
        if(!IsOwner) return;

        //If you unhid, sucks to be you! You lost!
        if(!localHiding)
        {
            //ADD: code to die! And tell the player checking the spot they won
            HiddenHit(true);
            return;
        }

        canHide = false;
        isPendingCheck = true;
        
        //This code is run when being checked
        //Have the question pop up and subscribe for an answer
        QuestionDisplay.instance.OnAnswer += PendCheckAnswer;
        QuestionDisplay.instance.DisplayQuestion(questionID, true);
        //Hide the timer from hiding
        QuestionDisplay.instance.DisplaySider(false);

        checkingPlayer = NetworkPlayerObject.playerObjectDictionary[checkerId];

        print("I better answer question: " + questionID);
    }

    [ClientRpc]
    public void LostCheckClientRpc(ulong winnerID, bool lostToPlayer)
    {
        //This is run when you answer too slow
        if(!IsOwner) return;
        QuestionDisplay.instance.HideQuestion();
        QuestionDisplay.instance.OnAnswer -= PendCheckAnswer;
        canHide = true;

        //Turn it back up! (the music)
        MusicManager.instance.SetLowPass(false, 1f);
        MusicManager.instance.FadeVolume(1f, 1f);

        if(localHiding)
        {
            HiddenHit(true);
            isPendingCheck = false;

            //Lets tell the Game log as the victim (offender doesn't know who they hit)
            GameLogManager.instance.AddVictimEntryOnline(winnerID, 0);

            return;
        }
        isPendingCheck = false;


        HitFromHidingSpot();
    }

    //The isTag here makes sure that you can't tag nothing
    [ClientRpc]
    public void WonCheckClientRpc(bool isTag)
    {
        //This is run when you answer fast enough and win
        canHide = true;
        isPendingCheck = false;

        if(!IsOwner) return;
        print("I WON!");

        //Hide the question if I hadn't answered yet
        QuestionDisplay.instance.HideQuestion();
        QuestionDisplay.instance.OnAnswer -= PendCheckAnswer;

        //Turn it back up! (the music)
        MusicManager.instance.SetLowPass(false, 1f);
        MusicManager.instance.FadeVolume(1f, 1f);

        //We add points accordingly
        if(GameManager.instance.gamemode == 0){
            if(currentTeamState == hider) PointsManager.instance.SetHASPoints(3);
        }

        //Run any special code (Checking player will always be null if we were the ones checking)
        if(isTag) currentTeamState.WhenTagging(this, checkingPlayer);
    }

    void PendCheckAnswer(object sender, QuestionDisplay.OnAnswerArgs e){
        //Tell the hiding spot the results
        print("I answered right: " + e.isRight);
        hidingGoal.VerifyAnswerServerRpc(OwnerClientId, e.time, e.isRight);

        //We add points to us depending on the gamemode and our team
        if(GameManager.instance.gamemode == 0){
            if(currentTeamState == hider) PointsManager.instance.SetHASPoints(0);
            if(currentTeamState == seeker) PointsManager.instance.SetHASPoints(1);
        }

        //Hide the displayed question
        QuestionDisplay.instance.HideQuestion();

        //Unsubscribe so this is only called once
        QuestionDisplay.instance.OnAnswer -= PendCheckAnswer;
    }

    #endregion

    #region VIOLENCE (being hit and hitting)
    public void BeViolent(){
        //This is only run on the Owner of the Player AND if they aren't already swinging or being hit
        if(movement.GetCurrentStateID() != 2 && movement.GetCurrentStateID() != 1)

        //Lets get the direction of the swing
        movement.ChangeState(2, true);

        //Changing the movement state does calls all the hit calculations for us, so we are good!
        
    }
    public void CheckThingsToHit(){
        //Check for players in front of us
        Vector2 currentposV2 = transform.position;
        Collider2D[] Hits = Physics2D.OverlapCircleAll(currentposV2 + (movement.lastVelDir * 0.75f), 1.2f);

        

        if(!CheckCollidersForHittablePlayers(Hits)){
            //IF we didn't hit anything, then we check around us
            Hits = Physics2D.OverlapCircleAll(transform.position, 0.9f);
            CheckCollidersForHittablePlayers(Hits);
        }
    }

    //This was created to shrink up the code and simplify it
    bool CheckCollidersForHittablePlayers(Collider2D[] Hits){
        foreach (var item in Hits)
        {
            Player hitplayer = item.GetComponent<Player>();
            if(hitplayer != null && hitplayer != this)
            {
                print("Hit player on team: " + ((int)hitplayer.currentTeamEnum.Value - 1));
                //Check if the player is on the right team to be hit
                bool isRightTeam = currentTeamState.hitableTeams.Contains((int)hitplayer.currentTeamEnum.Value);
                if(isRightTeam){
                    HitSomeoneServerRpc(OwnerClientId, hitplayer.OwnerClientId);
                    currentTeamState.WhenTagging(this, hitplayer);
                }

                //Lets show an effect
                EffectsManager.SpwanObjectFromPool(1, item.transform.position, Quaternion.identity);

                //If the player can't tag mutiple people it will just tag this person
                if(!currentTeamState.canTagMulti) return true;
            }
        }
        return false;
    }

    void DEBUG_DrawSphere(Vector3 pos, float radius){
        //DEBUG From website https://discussions.unity.com/t/drawing-spheres-at-runtime/16872
        GameObject mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mySphere.transform.localScale = Vector3.one * radius;
        mySphere.transform.position = pos;
        //END DEBUG 
    }
    [ServerRpc(RequireOwnership = false)]
    void HitSomeoneServerRpc(ulong HitterID, ulong hitPlayerID){
        //Run this script is run on every client on the player that was hit
        NetworkPlayerObject.playerObjectDictionary[hitPlayerID].HitClientRpc(HitterID);

        //WRITE HERE: Change the teams of those who were hit
    }

    [ClientRpc]
    void HitClientRpc(ulong HitterID){
        //This is run on every client on the player that was hit, code should be added for effects

        //We show ourselves IF we are hiding on the server, OR we are locally hidden and the owner
        if(isHiding.Value || (IsOwner && localHiding)){
            sprites.ChangeSpriteVisibility(true);
            rb.simulated = true;
            playerCollider.enabled = true;

            if(IsOwner){
                print("GLITCHY STATE!");
                //We hide the question with an override so it doesn't display
                QuestionDisplay.instance.OnAnswer -= HidingAnswer;
                QuestionDisplay.instance.HideQuestion(true);

                FullyUnhide();
            }
        }
        animator.SetFloat("M_Speed", 0f);
        
        

        if(!IsOwner) return;

        print("we have been hit!");

        movement.SetHitDir((transform.position - NetworkPlayerObject.playerObjectDictionary[HitterID].transform.position).normalized);
        currentTeamState.WhenTagged(this, true);
        return;
    }

    /// <summary>
    /// This is so we can add points accordingly when we tag someone
    /// This has to be run BEFORE changing teams
    /// </summary>
    public void TagAddPoints(){
        if(GameManager.instance.gamemode == 0){
            if(currentTeamState == seeker) PointsManager.instance.SetHASPoints(2);
        }
    }


    #endregion

    #region TeamManagement
    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(TeamsEnum teamValue)
    {
        //This code is only run server-side
        currentTeamEnum.Value = teamValue;
        Debug.Log("SERVER: Team changed to: " + currentTeamEnum.Value);
    }

    public void SetTeam(int teamID, bool startEnter = false)
    {
        print("Attempted team setup with team: " + teamID);
        //This code is run on the client

        //ADD MORE LINES HERE FOR MORE TEAMS

        if(currentTeamState != null) currentTeamState.LeaveTeam(this);

        if(teamID == 0){
            currentTeamState = noTeam;
            SetTeamServerRpc(TeamsEnum.NoTeam);
        }
        if(teamID == 1){
            currentTeamState = hider;
            SetTeamServerRpc(TeamsEnum.Hider);
        }
        if(teamID == 2){
            currentTeamState = seeker;
            SetTeamServerRpc(TeamsEnum.Seeker);
        }
        Debug.Log("Set Team ID to: " + teamID);
        
        currentTeamState.EnterTeam(this, startEnter);

        //Now we set our team color
        //ADD CODE THAT USES: TeamColorManager.teamColorDictionary[teamID]

        //Now we set our nickname to the right color
        playerNickname.ChangeNameColorLocal(teamID);

        //Now we ensure we are seeing the right team nicknames        
    }
    
    /// <summary>
    /// This tells the client to set their team. It is ONLY sent to the owner. And assums that this is a setup so it is a start enter
    /// </summary>
    /// <param name="teamID"></param>
    /// <param name="startEnter"></param>
    public void ServerSetTeam(int teamID, bool startEnter = true){
        clientRpc.Send.TargetClientIds = new List<ulong>{
            OwnerClientId
        };

        //We tell the client that their team has been set
        //We (hopefully) only send this to the client who owns this
        SetTeamClientRpc(teamID, startEnter, clientRpc);
    }

    [ClientRpc]
    void SetTeamClientRpc(int teamID, bool startEnter = false, ClientRpcParams rpcParams = default){
        print("I'VE HAD MY TEAM SET BY THE SERVER!!!");
        //We obey the servers request
        SetTeam(teamID, startEnter);

        //We tell the server we are done
        ConfirmTeamServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ConfirmTeamServerRpc(ServerRpcParams rpcParams = default){
        NetworkedGameManager.instance.ConfirmTask(rpcParams.Receive.SenderClientId);
    }

    void OnTeamChange(TeamsEnum previous, TeamsEnum current){
        //This is run on EVERY client when the text
        
        //Lets not bother with (most of) this if we are the owner
        if(IsOwner) {
            //We want to confirm names AFTER the server does it's job
            playerNickname.HideNameIfOnTeamsAll(currentTeamState.invisibleTeams);
            return;
        }

        //Lets change the player color first
        playerNickname.ChangeNameColorLocal((int)current);

        //We get the client this is running on's player object. And knowing their team we get what teams they should see
        List<int> clientsVisibleTeams = isRightPlayer.currentTeamState.invisibleTeams;

        //We take those teams and ask this player that just changed teams if their nickname should be visable or not
        playerNickname.HideNameIfOnTeams(clientsVisibleTeams);
    }

    public bool CanPointToTeam(int team){
        return currentTeamState.pointerTeams.Contains(team);
    }
    public int GetTeam(){
        return (int)currentTeamEnum.Value;
    }
    
    #endregion

    #region Interaction

    public void CheckInteract()
    {
        //Check for IInteractables in front (around) of us
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var item in colliders)
        {
            IInteractable interactable = item.GetComponent<IInteractable>();
            if(interactable != null)
            {
                interactable.Interact(OwnerClientId);
                return;
            }
        }
        
    }

    #endregion

    #region Disable Input

    public void CanBeInput(bool i){
        if(i){
            canPlay = true;
            canHide = true;
        }

        if(!i){
            if(localHiding && !PendingUnhide) FullyUnhide();

            canPlay = false;
            canHide = false;

            movement.ResetSpeed();
        }

        print("Can I play: " + canPlay);
    }

    void CanBeInputWithDelay(bool i, float delay){
        if(delay <= 0f){
            CanBeInput(i);
            return;
        }
        StartCoroutine(ChangeInputWithDelay(i, delay));
    }
    IEnumerator ChangeInputWithDelay(bool i, float delay){
        yield return new WaitForSecondsRealtime(delay);
        CanBeInput(i);
    }

    #endregion
    
    #region Spwan And Teleport
    
    public void ServerTeleport(bool toLobby){
        clientRpc.Send.TargetClientIds = new List<ulong>(){
            OwnerClientId
        };

        TeleportYourselfClientRpc(toLobby, clientRpc);
    }

    [ClientRpc]
    void TeleportYourselfClientRpc(bool toLobby, ClientRpcParams clientRpc = default){
        
        TeleportPlayer(toLobby);

        //Tell the server we are done
        ConfirmTeleportServerRpc();
    }

    [ServerRpc]
    void ConfirmTeleportServerRpc(ServerRpcParams rpcParams = default){
        NetworkedGameManager.instance.ConfirmTask(rpcParams.Receive.SenderClientId);
    }
    void TeleportPlayer(bool toLobby){
        if(toLobby){
            transform.position = SpwanManager.instance.GetLobbySpwan();
        }else{
            transform.position = SpwanManager.instance.GetGameSpwan();
        }
        //Now we tell the server to stop interpolation on the clients (Interpolate is auto-synced)
        StartCoroutine(StopSmoothMovement());
    }

    IEnumerator StopSmoothMovement(){
        netTransform.Interpolate = false;
        yield return new WaitForSecondsRealtime(1f / (float)NetworkManager.NetworkTickSystem.TickRate);
        netTransform.Interpolate = true;

    }
    
    #endregion
    
    #region Disconnect
    void OnClientDisconnect(ulong obj)
    {
        print("A CLIENT HAS DISCONNECTED! REMOVEING THEM FROM LIST!!!");
        NetworkPlayerObject.Singleton.DisconnectClient(obj);
    }
    #endregion


    //DEBUG!!! REMOVE AFTER DONE!!!
    public bool IsHider(){
        return currentTeamEnum.Value == TeamsEnum.Hider;
    }

}
