using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// THIS IS TEAM ID: 1
public class Team_Hider : Team
{
    public override bool canTagMulti {get; set;} = false;
    public override bool canBeTagged { get; set; } = true;
    public override float speed { get; set; } = 4f;
    public override List<int> invisibleTeams {get; set;} = new List<int>();
    public override List<int> hitableTeams {get; set;} = new List<int>();
    public override List<int> pointerTeams {get; set;} = new List<int>(){
        2,
    };

    float speedTimer;
    float speedMutiplyer = 1f;


    public override void EnterTeam(Player player, bool startEnter)
    {
        speedTimer = 0f;
        speedMutiplyer = 1f;

        player.SetTimer(Player.maxTimerInSpot);

        if(startEnter) return;
        //Only run code here if you entered this team mid-game

        speedTimer = 5f;
        speedMutiplyer = 1.4f;
        player.playerEffects.StartEffectNetworked(1);
    }

    public override void LeaveTeam(Player player)
    {
        player.playerEffects.StopEffectNetworked(1);
    }

    public override void UpdateTeam(Player player)
    {
        if(player.movement.isHit) return;
        //All code below here is only run if the player isn't hit

        //This is up here because it is run even if the wrong answer is being displayed
        player.HideTimerUpdate();

        //Don't move or unhide if you are being shown a wrong answer
        if(QuestionDisplay.instance.showingWrong) return;

        player.hidingInputs();


        //If the timer is going, then count down and reset speed
        if(speedTimer > 0f)
        {
            speedTimer -= Time.deltaTime;
            if(speedTimer <= 0f) {
                speedMutiplyer = 1f;
                player.playerEffects.StopEffectNetworked(1);
            }
        }
    }

    public override void FixedUpdateTeam(Player player)
    {
        //only move if not hiding
        if(player.localHiding) return;

        //This moves the player. Automatically handles getting hit
        player.FixedMoveWithSpeed(speed * speedMutiplyer);
    }

    public override void WhenHide(Player player)
    {
        player.playerEffects.StopEffectNetworked(1);
    }

    public override void WhenTagged(Player player, bool teamChanger)
    {
        //Set the team to the seeker if you were checked
        if(teamChanger){
            player.SetTeam(2);
        }

        //Otherwise, just get hit in a direction
        player.ChangeMovementState(1, true);
    }

    public override void WhenTagging(Player player, Player taggedPlayer)
    {
        //This is run when you successfully beat a seeker that is checking your spot

        //Points were already added

        //Next, Unhide
        player.FullyUnhide();

        //Next, apply a speed boost
        speedMutiplyer = 1.5f;
        speedTimer = 3f;

        //Now we add it to the leaderboard
        GameLogManager.instance.AddEntryOnline(taggedPlayer.OwnerClientId, 1);
    }

    public override void WhenUnhide(Player player)
    {
        
    }
}
