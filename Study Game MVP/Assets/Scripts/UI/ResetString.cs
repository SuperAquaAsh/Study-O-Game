using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetString : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    string goal = "";
    private void Awake() {
        goal = text.text;
    }
    public void RevertString(){
        text.text = goal;
    }
}
