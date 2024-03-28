using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizItemDelete : MonoBehaviour
{
    [HideInInspector] public static QuizItemDelete instance;

    [SerializeField] GameObject deleteGO;

    [SerializeField] TextMeshProUGUI deleteText;


    string deleteQuizLocation = "";
    int deleteQuestionID = -1;

    GameObject deleteButton;
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null){
            instance = this;
        }
        else{
            Debug.LogWarning("Two instances of 'QuizItemDelete' found!");
            Destroy(this);
            return;
        }

        deleteGO.SetActive(false);
    }

    public void RequestQuizDelete(string quizLocation, GameObject quizButton, string quizName = "this"){
        //Remeber the Quiz and button to delete (if we do)
        deleteQuizLocation = quizLocation;

        //Remember the button to delete
        deleteButton = quizButton;

        //If shift is being held down then skip the next steps and just delete
        if(Input.GetKey(KeyCode.LeftShift)){
            ConfirmDelete();
            return;
        }

        //Set the text of the delete confirm
        if(quizName == "") quizName = "this";
        deleteText.text = "Are you sure you want to delete the quiz: " + quizName + "?";
        
        //Spwan in the button to double check (Only do this IF the person isn't holding down shift)
        deleteGO.SetActive(true);
    }
    public void RequestQuestionDelete(int questionID, string questionName = "this"){
        //remember the question to delete
        deleteQuestionID = questionID;

        //Set the text of the delete confirm
        if(questionName == "") questionName = "this";
        deleteText.text = "Are you sure you want to delete the question: " + questionName + "?";

        //Spwan in the button to double check
        deleteGO.SetActive(true);
    }

    public void ConfirmDelete(){
        //Hide the confirm button
        deleteGO.SetActive(false);
        
        bool successfulDelete = false;

        //Delete the quiz (if it exists)
        if(deleteQuizLocation != ""){
            successfulDelete = QuizSaver.instance.DeleteQuiz(deleteQuizLocation);
            deleteQuizLocation = "";
        }

        //Delete the question (if it exists)
        if(deleteQuestionID != -1){
            //DELETE THE QUESTION
            QuizMaker.instance.DeleteQuestion(deleteQuestionID);
            deleteQuestionID = -1;
        }

        
        //Make sure to delete the button (if it exists & if we even deleted anything)
        if(deleteButton != null && successfulDelete){
            Destroy(deleteButton);
            deleteButton = null;
        }
    }
    public void CancelDelete(){
        //Hide the confirm button
        deleteGO.SetActive(false);

        deleteQuestionID = -1;
        deleteQuizLocation = "";
    }
}
