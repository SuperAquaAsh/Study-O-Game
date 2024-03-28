using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionsSlider : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void OnSliderChange(Single v){
        text.text = v.ToString();

        GameManager.instance.SetConnectionNumber((int)v);
    }
}
