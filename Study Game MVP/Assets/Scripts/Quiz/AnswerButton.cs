using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//All this does is tell things when it was clicked and sends an answer out
public class AnswerButton : MonoBehaviour
{
    public bool isCorrect;
    bool canAnswer;
    [SerializeField] Image buttonImage;
    [SerializeField] TextMeshProUGUI buttonText;
    
    private void Start() {
        if(buttonText == null) buttonImage = GetComponent<Image>();
        if(buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void ClickedOnAnswer(){
        if(canAnswer && QuestionDisplay.instance.timer > 0.4f) QuestionDisplay.instance.Answered(isCorrect);
        return;
    }
    public void ChangeDisplay(bool showValue = true){
        if(showValue){
            if(isCorrect) buttonImage.color = Color.green;
            if(!isCorrect) buttonImage.color = Color.red;
            canAnswer = false;
        }else{
            buttonImage.color = Color.white;
        }
    }
    public void ChangeText(string newText){
        buttonText.text = newText;
        buttonImage.color = Color.white;
    }
    public void SetAnswer(string newText, bool isRight){
        buttonText.text = newText;
        isCorrect = isRight;
        canAnswer = true;

        buttonImage.color = Color.white;
    }
    public void SetCanAnswer(bool buttonCanAnswer){
        canAnswer = buttonCanAnswer;
    }
}
