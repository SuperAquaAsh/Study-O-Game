using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardObject : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI pointsText;

    public void SetData(ulong playerId, uint points){
        nameText.text = PlayerNickname.playerIdName[playerId];
        pointsText.text = points.ToString();
    }
}
