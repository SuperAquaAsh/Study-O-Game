using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuizButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] TextMeshProUGUI text;

    [SerializeField] Image deleteImage;
    [SerializeField] bool isSetup;

    string saveLocation = "";

    string quizName;

    //These are for selection of quizzes before the game only
    GameObject currentMenu;
    GameObject nextMenu;

    private void Start() {
        //First, lets set the text back to the original size
        text.margin = new Vector4(text.margin.x, text.margin.y, 10, text.margin.w);

        //Next, lets hide the button
        if(deleteImage != null) deleteImage.color = new Color(deleteImage.color.r, deleteImage.color.g, deleteImage.color.b, 0);
    }

    public void LoadQuiz(){
        QuizMaker.instance.LoadQuiz(saveLocation);
    }
    //This just tells the Game Manager what Quiz the host is choosing
    public void SetupGameQuizLocation(){
        GameManager.instance.SetQuizLocation(saveLocation);
        GameManager.instance.SetQuizName(quizName);
        //Also despwan the current area and spwan in the new one
        currentMenu.SetActive(false);
        nextMenu.SetActive(true);
    }

    //This sets data so we can transition between menus
    public void SetMenuData(GameObject current, GameObject next){
        isSetup = true;
        currentMenu = current;
        nextMenu = next;
    }

    public void SetQuizData(string location, string name){
        saveLocation = location;
        quizName = name;

        if(quizName != "") text.text = quizName;
        else text.text = "SAVED AT: " + location;        
    }

    public void RequestDelete(){
        QuizItemDelete.instance.RequestQuizDelete(saveLocation, gameObject, quizName);
    }

    public void OnPointerEnter(PointerEventData eventData){
        if(isSetup) return;

        //First, lets shrink up the text
        text.margin = new Vector4(text.margin.x, text.margin.y, 70, text.margin.w);

        //Next, lets show the button
        deleteImage.color = new Color(deleteImage.color.r, deleteImage.color.g, deleteImage.color.b, 255);
    }

    public void OnPointerExit(PointerEventData eventData){
        if(isSetup) return;
        
        //First, lets set the text back to the original size
        text.margin = new Vector4(text.margin.x, text.margin.y, 10, text.margin.w);

        //Next, lets hide the button
        deleteImage.color = new Color(deleteImage.color.r, deleteImage.color.g, deleteImage.color.b, 0);
    }
}
