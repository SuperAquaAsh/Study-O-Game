using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// This handels the player name and color (color of name and player)
/// </summary>
public class PlayerNickname : NetworkBehaviour
{
    
    NetworkVariable<FixedString32Bytes> networkName = new NetworkVariable<FixedString32Bytes>();

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject background;

    public Player player;

    [SerializeField] SpriteRenderer bodyFiller;

    public static List<PlayerNickname> allPlayerNicknames {get; private set;} = new List<PlayerNickname>();

    public static Dictionary<ulong, string> playerIdName {get; private set;} = new Dictionary<ulong, string>();
    // Start is called before the first frame update

    public override void OnNetworkSpawn(){
        //We clear our static variables if they were set from a previous game (IsOwner ensuers it is only run once)
        playerIdName.Add(OwnerClientId, networkName.Value.ToString());

        //We add this to our list to keep track of every player object
        allPlayerNicknames.Add(this);
        
        networkName.OnValueChanged += OnNicknameChange;
        text.text = networkName.Value.ToString();
        SetNicknameLocal(SettingsManager.instance.currentSettings.nickname);
    }

    #region Set Data and OnChange
    public void SetNicknameLocal(string nickname){
        //We only want to do this locally
        if(!IsOwner) return;

        text.text = nickname;
        print("setting name to: " + nickname);
        SetNicknameServerRpc(nickname);
    }

    [ServerRpc]
    void SetNicknameServerRpc(string nickname){
        networkName.Value = new FixedString32Bytes(nickname);
    }

    void OnNicknameChange(FixedString32Bytes previous, FixedString32Bytes current){
        text.text = current.ToString();

        if(playerIdName.ContainsKey(OwnerClientId)) playerIdName.Remove(OwnerClientId);

        playerIdName.Add(OwnerClientId, current.ToString());
        if(!playerIdName.ContainsKey(OwnerClientId)) playerIdName.Add(OwnerClientId, current.ToString());
    }

    //The data is reset when we leave, not join
    public static void ResetData(){
        playerIdName.Clear();
        allPlayerNicknames.Clear();
    }

    #endregion

    public void ChangeNameColorLocal(int teamId){
        text.color = TeamColorManager.teamColorDictionary[teamId];

        //Also set the body Color
        bodyFiller.color = TeamColorManager.teamFillerColorDictionary[teamId];
    }

    public void HideNameIfOnTeams(List<int> effectedTeams){
        if(effectedTeams.Contains((int)player.currentTeamEnum.Value)){
            text.gameObject.SetActive(false);
            background.SetActive(false);
        }else if(text != null){
            text.gameObject.SetActive(true);
            background.SetActive(true);
            text.alpha = 100;
        }
    }

    /// <summary>
    /// This hides the nicknames of players that are on the effectedTeams list. This checks EVERY PlayerNickname
    /// </summary>
    /// <param name="effectedTeams">The teams that will be visable</param>
    public void HideNameIfOnTeamsAll(List<int> effectedTeams){
        foreach (var item in allPlayerNicknames)
        {
            //We hide each nickname accordingly, but we will ALWAYS see our name
            if(item != this) item.HideNameIfOnTeams(effectedTeams);
            else {
                text.gameObject.SetActive(true);
                background.SetActive(true);
                text.alpha = 100;
            }
        }
    }
}