using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Hiding : NetworkBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;


    public NetworkVariable<bool> isHiding;
    public NetworkVariable<bool> isBeingChecked;
    public NetworkVariable<bool> isEnabled = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private ulong[] targetClients = new ulong[1];

    float lastHideTick;
    ulong waitPlayerID;
    ulong lastAnswerID;

    //This represents the last time someone answers the question
    float lastCheckTime;
    public NetworkVariable<ulong> lastHideID;

    //These are for checking spots and storing state
    ulong checkerID;
    bool hasAnsweredChecker = false;
    bool isCorrectChecker;
    float checkerAnswerTime = 0f;
    bool hasAnsweredHider = false;
    bool isCorrectHider;
    float hiderAnswerTime = 0f;

    private void Start() {
        isHiding.OnValueChanged += OnHidingChange;
        isEnabled.OnValueChanged += OnEnableChange;
    }
    private void Update()
    {
        if(!IsClient) return;
        return;
    }

    void IInteractable.Interact(ulong PlayerID){
        //This code only runs on the client that interacted with the hiding spot

        //If storing a player or being checked, don't even bother trying to hide here
        if(isHiding.Value || isBeingChecked.Value)
        {
            NetworkPlayerObject.playerObjectDictionary[PlayerID].FailedHidingClientRpc();
            return;
        }

        //Set this to the players Goal hiding spot locally, which does a bit of client-side prediction and assumes they already have the spot
        NetworkPlayerObject.playerObjectDictionary[PlayerID].SetGoalHidingSpot(this);

        //Send a request to the server to see if the player can hide
        CheckAvaibilityServerRpc(PlayerID, (NetworkManager.LocalTime.TimeAsFloat + NetworkManager.ServerTime.TimeAsFloat)/2);
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckAvaibilityServerRpc(ulong PlayerID, float gameTime){
        //A request for a client to hid is sent to the server
        Player player = NetworkPlayerObject.playerObjectDictionary[PlayerID];
        Player lastPlayer = NetworkPlayerObject.playerObjectDictionary[lastHideID.Value];

        //If someone is hiding server-end, AND they were hiding first, then the person who is attempting has failed. But if no one is hiding server end or
        //the person who currently wants to hide acttually hid first, then the person inside the spot is kicked out and the new person moves in.
        if((isHiding.Value || isBeingChecked.Value) && gameTime > lastHideTick && PlayerID != lastHideID.Value)
        {
            player.FailedHidingClientRpc();
            return;
        }
        
        //If anyone is in the hiding spot, kick em' out!
        if(isHiding.Value){
            isHiding.Value = false;
            lastPlayer.FailedHidingServerSide();
            lastPlayer.FailedHidingClientRpc();
        }


        //Set the last time that someone had entered the spot to when the request was sent
        lastHideTick = gameTime;

        //Set the new hider to the last ID who has hidden here
        lastHideID.Value = PlayerID;

        //Set this to currently hiding someone
        isHiding.Value = true;

        //We set the server hiding stuff (if we aren't the server)
        player.ChangeHidingServerSide(true);

        //Successfull hiding is sent to all clients on one player
        player.HideClientRpc();

        //Set The goal hiding spot on the server, this is so that clients can unhide on the server-side (Not if we are already the server)
        if(player.OwnerClientId != OwnerClientId) player.SetGoalHidingSpot(this);
    }

    [ServerRpc(RequireOwnership = false)]
    void UnhideServerRpc(){
        isHiding.Value = false;
    }

    public void Unhide(){
        UnhideServerRpc();
    }

    void OnHidingChange(bool previous, bool current)
    {
        if(current)
        {
            //Put code here to play a hiding animation
            return;
        }

        //put code here to play an unhiding animation
    }

    public bool CheckPendingHidingSpot(ulong PlayerID){
        //This code is run by the client when they check a spot

        //If the spot is already being checked by another player then just give up
        if(isBeingChecked.Value)
        {
            return false;
        }

        CheckHidingSpotServerRpc(PlayerID);
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckHidingSpotServerRpc(ulong PlayerID){
        //A request by the client to check a hiding spot

        //If someone is checking server-end, AND they were checking first, then the person who is attempting has failed. But if no one is checking server end or
        //the person who currently wants to check acttually checked first, then the person checking the spot is stopped and the new person moves in.
        
        //This just prefers people with faster internet, not too important
        if(isBeingChecked.Value)
        {
            //We get the params to tell the required player that they were "Your too slow!" "Your too slow!" "Your too slow!" "Your too slow!"
            targetClients = new ulong[]{PlayerID};

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targetClients
                }
            };

            NetworkPlayerObject.playerObjectDictionary[PlayerID].FailedSpotCheckClientRpc(clientRpcParams);
            return;
        }
        //All code bellow here is run if the player is successfully checking, but remember it can be canceled

        //Start the Couroutine that will handle the requests
        StartCoroutine(HandleCheck(PlayerID));        
    }
    [ServerRpc (RequireOwnership = false)]
    public void VerifyAnswerServerRpc(ulong PlayerID, float answerTime, bool correctAnswer)
    {
        //This is sent out when a client has successfully answered a question and believes that they answered it first
        //It will update variables for the courotine

        //If not being checked don't even bother
        if(!isBeingChecked.Value) return;

        //If this is the player that is checking, then set the Checker variables
        if(PlayerID == checkerID)
        {
            isCorrectChecker = correctAnswer;
            checkerAnswerTime = answerTime;
            hasAnsweredChecker = true;
            return;
        }

        //If this is the player that is hiding, then set the Hider variables
        if(PlayerID == lastHideID.Value)
        {
            isCorrectHider = correctAnswer;
            hiderAnswerTime = answerTime;
            hasAnsweredHider = true;
            return;
        }

        Debug.LogWarning("An ID sent a request to answer a question, without being the offical checker");
        return;
    }

    IEnumerator HandleCheck(ulong PlayerID){
        //MASSIVE ERROR!!! If one of players disconnects while this is happening then we are completely screwed

        #region It first inisiates everything by sending out questions and whatnot to players
        //Now I am being checked
        isBeingChecked.Value = true;

        //Set the ID of the Person Checking
        checkerID = PlayerID;

        //Generate the question to answer (ADD IT TO ACTUALLY BE THE MAX AMMOUNT OF QUESTIONS)
        int questionID = UnityEngine.Random.Range(0, QuizManager.instance.quizLength);

        //This code notifies the player that is checking what question to answer
        NetworkPlayerObject.playerObjectDictionary[PlayerID].SuccessfullSpotCheckClientRpc(questionID);

        //This notifies the player in the spot to answer a question. make sure to check if you are actually hiding someone
        if(isHiding.Value) NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].BeingCheckedClientRpc(questionID, PlayerID);
        #endregion
        
        #region Then, it waits for a response from one of the Players. Then does stuff if anyone answered wrong
        //It waits until a Player Has answered the question. There are two bools per player, hasAnswered, and isCorrect
        while (!hasAnsweredChecker && !hasAnsweredHider) yield return null;
        //This code is run when someone has answered

        //If the checker answered and is wrong, just tell everyone who won
        if(hasAnsweredChecker && !isCorrectChecker)
        {
            //Could maybe remove sending it to the person who lost, they know they lost
            NetworkPlayerObject.playerObjectDictionary[checkerID].LostCheckClientRpc(lastHideID.Value, isHiding.Value);
            if(isHiding.Value) NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].WonCheckClientRpc(true);
            ResetCheckVariables();
            yield break;
        }

        //If the hider answered and is wrong, just tell everyone who won
        if(hasAnsweredHider && !isCorrectHider)
        {
            //Could maybe remove sending it to the person who lost, they know they lost
            NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].LostCheckClientRpc(checkerID, true);
            NetworkPlayerObject.playerObjectDictionary[checkerID].WonCheckClientRpc(true);
            ResetCheckVariables();
            yield break;
        }
        #endregion

        #region The courotine will now wait for the respose from the next player (if necessary)
        
        //If no one is hiding in this spot then just send out the answer
        if(!isHiding.Value)
        {
            NetworkPlayerObject.playerObjectDictionary[checkerID].WonCheckClientRpc(false);
            ResetCheckVariables();
            yield break;
        }
        
        float timer = 0.5f;

        //If the Checker hadn't answered, then we will wait for them
        if(!hasAnsweredChecker && timer == 0.5f)
        {
            while(timer > 0f && !hasAnsweredChecker){
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        //If the Hider hadn't answered, then we will wait for them
        if(!hasAnsweredHider && timer == 0.5f)
        {
            while(timer > 0f && !hasAnsweredHider){
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        
        //Timer will equal zero if someone hadn't answered, and their bool wouldn't have changed
        #endregion

        //ADD CODE HERE: Expand on the players checking spots by using the results to send out who won and lost
        #region This code is run if some did't answer. Thats naughty, so they get disiplined
        
        //This code here is if the timer ran out on one player
        if(timer <= 0f && (!hasAnsweredChecker || !hasAnsweredHider))
        {
            //If it was the Checker that hadn't answered, slap them on the wrist
            if(!hasAnsweredChecker)
            {
                NetworkPlayerObject.playerObjectDictionary[checkerID].LostCheckClientRpc(lastHideID.Value, true);
                NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].WonCheckClientRpc(true);
                ResetCheckVariables();
                yield break;
            }

            //If it was the Hider that hadn't answered, slap them on the wrist
            if(!hasAnsweredHider)
            {
                NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].LostCheckClientRpc(checkerID, true);
                NetworkPlayerObject.playerObjectDictionary[checkerID].WonCheckClientRpc(true);
                ResetCheckVariables();
                yield break;
            }
            print("Broke because ERROR");
            ResetCheckVariables();
            yield break;
        }
        print("I present: " +timer);

        #endregion

        #region This code calculates the winner based on the time it took to answer the question

        print("Seeker time: " + checkerAnswerTime + "  Hider Time: " + hiderAnswerTime);
        
        //If the hider answered in less time
        if(checkerAnswerTime > hiderAnswerTime)
        {
            NetworkPlayerObject.playerObjectDictionary[checkerID].LostCheckClientRpc(lastHideID.Value, true);
            NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].WonCheckClientRpc(true);
            ResetCheckVariables();
            yield break;
        }

        //If the Checker answered in less time
        if(hiderAnswerTime > checkerAnswerTime)
        {
            NetworkPlayerObject.playerObjectDictionary[lastHideID.Value].LostCheckClientRpc(checkerID, true);
            NetworkPlayerObject.playerObjectDictionary[checkerID].WonCheckClientRpc(true);
            ResetCheckVariables();
            yield break;
        }

        #endregion

        ResetCheckVariables();
        yield break;
    }

    void ResetCheckVariables(){
        isBeingChecked.Value = false;
        checkerID = 0;
        hasAnsweredChecker = false;
        isCorrectChecker = false;
        checkerAnswerTime = 0f;
        hasAnsweredHider = false;
        isCorrectHider = false;
        hiderAnswerTime = 0f;
    }

    #region On Enablence
    void OnEnableChange(bool p, bool c){
        GetComponent<Collider2D>().enabled = c;
        spriteRenderer.enabled = c;
    }

    public void SetEnable(bool v){
        if(!IsHost) return;
        isEnabled.Value = v;
    }

    #endregion
}
