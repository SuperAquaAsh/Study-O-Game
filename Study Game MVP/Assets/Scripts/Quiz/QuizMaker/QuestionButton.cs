using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image image;
    [SerializeField] Color selectedColor;
    [SerializeField] Image warningImage;
    [SerializeField] Image deleteImage;
    int questionID;
    // Update is called once per frame
    public void SetQuestion(string setText, int QuestionID){
        questionID = QuestionID;

        if(setText != ""){
            text.text = setText;
        }
        else text.text = "NO QUESTION";
    }

    public void ChangeQuestion(){
        QuizMaker.instance.ChangeQuestion(questionID);
    }
    public string GetText(){
        return text.text;
    }
    public int GetID(){
        return questionID;
    }
    public void SetID(int ID){
        questionID = ID;
    }

    #region Display things

    public void SetDisplayAnswer(){
        image.color = selectedColor;
    }
    public void UnSetDisplayAnswer(){
        image.color = Color.white;
    }

    public void SetWarning(){
        warningImage.gameObject.SetActive(true);
    }
    public void UnSetWarning(){
        warningImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData){
        //Lets show the button
        deleteImage.color = new Color(deleteImage.color.r, deleteImage.color.g, deleteImage.color.b, 255);
    }

    public void OnPointerExit(PointerEventData eventData){
        //Lets hide the button
        deleteImage.color = new Color(deleteImage.color.r, deleteImage.color.g, deleteImage.color.b, 0);
    }

    #endregion

    public void RequestDelete(){
        QuizItemDelete.instance.RequestQuestionDelete(questionID, text.text);
    }
}
