using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Team
{
    public abstract bool canTagMulti {get; set;}
    public abstract bool canBeTagged {get; set;}
    public abstract float speed {get; set;}
    public abstract List<int> invisibleTeams {get; set;}
    public abstract List<int> hitableTeams {get; set;}
    
    /// <summary>
    /// This tells us what teams we can see with team pointers
    /// </summary>
    public abstract List<int> pointerTeams {get; set;}

    public abstract void UpdateTeam(Player player);
    public abstract void FixedUpdateTeam(Player player);
    //startEnter basically means that the player started as this team
    public abstract void EnterTeam(Player player, bool startEnter);
    public abstract void LeaveTeam(Player player);

    public abstract void WhenHide(Player player);
    public abstract void WhenUnhide(Player player);
    public abstract void WhenTagged(Player player, bool teamChanger);
    public abstract void WhenTagging(Player player, Player taggedPlayer);

}
