using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS IS TEAM ID: 0
public class Team_NoTeam : Team
{
    public override bool canTagMulti {get; set;} = false;
    public override bool canBeTagged { get; set; } = true;
    public override float speed { get; set; } = 4f;

    //This just includes a giant list, if more than ten teams are added then this needs to be changed
    public override List<int> invisibleTeams {get; set;} = new List<int>();
    public override List<int> hitableTeams {get; set;} = new List<int>();
    public override List<int> pointerTeams {get; set;} = new List<int>();


    public override void EnterTeam(Player player, bool startEnter)
    {
        player.SetTimer(Player.maxTimerInSpot);
    }

    public override void LeaveTeam(Player player)
    {
        
    }

    public override void UpdateTeam(Player player)
    {

    }

    public override void FixedUpdateTeam(Player player)
    {
        //This moves the player
        player.FixedMoveWithSpeed(speed);
    }

    public override void WhenHide(Player player)
    {
        
    }

    public override void WhenTagged(Player player, bool teamChanger)
    {

    }

    public override void WhenTagging(Player player, Player taggedPlayer)
    {

    }

    public override void WhenUnhide(Player player)
    {
        
    }
}
