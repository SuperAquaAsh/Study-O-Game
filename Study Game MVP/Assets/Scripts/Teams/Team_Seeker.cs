using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS IS TEAM ID: 2
public class Team_Seeker : Team
{
    public override bool canTagMulti {get; set;} = false;
    public override bool canBeTagged { get; set; } = false;
    public override List<int> invisibleTeams {get; set;} = new List<int>(){
        1,
    };
    public override List<int> hitableTeams {get; set;} = new List<int>(){
        1,
    };

    public override List<int> pointerTeams {get; set;} = new List<int>(){};

    float baseSpeed = 4.05f;
    float maxSpeed = 4.5f;
    public override float speed { get; set; } = 4.2f;

    public override void EnterTeam(Player player, bool startEnter)
    {
        speed = baseSpeed;

        if(startEnter) return;
        //All code bellow here is only run if the team changed midgame

        player.ChangeMovementState(1, true);
        player.ToggleHideTimer(false);
    }

    public override void LeaveTeam(Player player)
    {
        speed = baseSpeed;
    }

    public override void UpdateTeam(Player player)
    {
        if(player.isPendingCheck) return;
        //All code bellow here is only run if the player isn't checking a spot

        if(player.movement.isHit) return;
        //All code bellow here is only run if the player isn't hit

        //Don't even worry about moving and whatnot if you are being shown your mistake
        if(QuestionDisplay.instance.showingWrong) return;

        //For the player to commit VIOLENCE AGAINST HIS BRETHREN
        if(Input.GetKeyDown(KeyCode.Space) && Application.isFocused)
        {
            player.BeViolent();
        }

        //Code for the player to check spots (Only if we are focused and in normal movement)
        if(Input.GetKeyDown(KeyCode.F) && Application.isFocused && player.movement.GetCurrentStateID() == 0){
            player.PendHidingSpotCheck();
        }

        //Increase the speed a bit (0.01 per second OR 0.6 per minute)
        speed += Time.deltaTime * 0.01f;
        speed = Mathf.Clamp(speed, baseSpeed, maxSpeed);
    }
    public override void FixedUpdateTeam(Player player)
    {
        if(player.isPendingCheck) return;
        //All code bellow here is only run if the player isn't checking a spot

        //This moves the player. Automatically handles getting hit
        player.FixedMoveWithSpeed(speed);
    }

    public override void WhenHide(Player player)
    {
        Debug.LogWarning("HOW U HIDE ON TEAM SEEKER?");
    }

    public override void WhenTagged(Player player, bool teamChanger)
    {
        //Happens when the player in a hiding spot answers faster
        Debug.Log("Seeker was hit!");
        player.ChangeMovementState(1, true);
    }

    public override void WhenTagging(Player player, Player taggedPlayer)
    {
        //We add points FIRST (team changing will screw this up)
        player.TagAddPoints();
        //Set the team to hider
        player.SetTeam(1);

        player.movement.ChangeState(0, true);

        //Now we add it to the leaderboard (if it's not null)
        if(taggedPlayer != null) GameLogManager.instance.AddEntryOnline(taggedPlayer.OwnerClientId, 0);
    }

    public override void WhenUnhide(Player player)
    {
        
    }
}
