using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//This changes the text so the user knows what they set the time to
//AND sets the timer to what it should be in the GameManager
public class TimerSet : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void OnSliderChange(Single v){
        text.text = IntToTime((int)v);

        GameManager.instance.SetTimer((int)v);
    }

    String IntToTime(int v){
        int minutes = 0;
        while(v >= 60){
            v -= 60;
            minutes++;
        }
        string sec = v.ToString();
        if(sec.Length == 1){
            sec = "0" + sec;
        }

        return minutes + ":" + sec;
    }
}
