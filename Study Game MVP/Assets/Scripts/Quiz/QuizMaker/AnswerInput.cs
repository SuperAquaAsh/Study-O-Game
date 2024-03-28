using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerInput : MonoBehaviour
{
    public int AnswerID;

    public TMP_InputField inputField;
    public Toggle toggle;
    public void SetAnswerText(string text){
        print("Set answer!1!");
        QuizMaker.instance.SetAnswerText(text, AnswerID);
    }
    public void SetAnswerBool(bool value){
        QuizMaker.instance.SetAnswerBool(value, AnswerID);
    }
}
