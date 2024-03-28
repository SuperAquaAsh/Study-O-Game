using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

//This handles that area to the side of the scroll Area
public class QuizMakerScrollArea : MonoBehaviour
{
    [SerializeField] GameObject questionButtonPrefab;
    [SerializeField] ScrollRect rect;

    List<GameObject> buttons = new List<GameObject>();

    QuestionButton currentButton;
    
    public void UpdateFromQuiz(){
        //This shouldn't be required, but nice when iniciating things

        //Access the Quiz
        Question[] questions = QuizMaker.instance.GetQuiz().questions;

        //Update ALL of the list accordingly

        //Delete all the old objects
        if(buttons.Count > 0)
        {
            //Lets make a new list to check things from (Required because removing things from a list while it's in a loop causes bugs)
            List<GameObject> newButtons = new List<GameObject>(buttons.Count);
            foreach (var item in buttons)
            {
                newButtons.Add(item);
            }

            foreach (var item in newButtons)
            {
                buttons.Remove(item);
                Destroy(item);
            }
        }

        //Spwan in new objects
        for (int i = 0; i < questions.Length; i++)
        {
            //Add button to world and list
            GameObject gameObject = Instantiate(questionButtonPrefab, transform);
            buttons.Add(gameObject);

            //Edit the buttons values
            QuestionButton item = gameObject.GetComponent<QuestionButton>();
            item.SetQuestion(questions[i].questionText, i);
            
            //Set the current button to this and then check if the answers are right
            currentButton = item;
            SetQuestionWarning(QuizMaker.instance.CheckForCorrectAnswer(i));
        }

        //Set the question to what the current question is according to the QuizMaker
        SetQuestionButton(QuizMaker.instance.GetCurrentQuestion());
        SetQuestionWarning(QuizMaker.instance.CheckForCorrectAnswer());
    }
    public void AddQuestion(){
        //This adds a question to the list
        //MUST be run AFTER the question is added to the editingQuiz variable

        Question[] questions = QuizMaker.instance.GetQuiz().questions;
        //Add button to world and list
        GameObject gameObject = Instantiate(questionButtonPrefab, transform);
        buttons.Add(gameObject);

        //Edit the buttons values
        QuestionButton item = gameObject.GetComponent<QuestionButton>();
        item.SetQuestion(questions[questions.Length - 1].questionText, questions.Length - 1);
        

        //Set the button question
        SetQuestionButton(questions.Length - 1);
        //Now check for right answers in the current question
        SetQuestionWarning(QuizMaker.instance.CheckForCorrectAnswer());

        //Set the rect to be at the bottom
        //transform.localPosition = new Vector3(0f, transform.childCount * 150f, 0f);
    }

    public void DeleteQuestion(int questionID){
        //Create a clone of buttons because we will be editing that list
        //List<GameObject> newButtons = new List<GameObject>(buttons.Count);
        //foreach (var item in buttons)
        //{
            //newButtons.Add(item);
        //}

        GameObject deleteButton = null;
        //Remove the deleted question button from the list and the world
        foreach (var item in buttons)
        {
            if (item.GetComponent<QuestionButton>().GetID() == questionID){deleteButton = item;}
        }

        if(deleteButton != null){
            print("deleting button");
            if(deleteButton.GetComponent<QuestionButton>() == currentButton) currentButton = null;
            buttons.Remove(deleteButton);
            Destroy(deleteButton);
        }

        //Then, give every button after that questionID one lower of an ID
        foreach (var item in buttons)
        {
            int buttonID = item.GetComponent<QuestionButton>().GetID();
            if(buttonID > questionID){
                item.GetComponent<QuestionButton>().SetID(buttonID - 1);
            }
        }

        //Set the question to what the current question is according to the QuizMaker
        SetQuestionButton(QuizMaker.instance.GetCurrentQuestion());
        SetQuestionWarning(QuizMaker.instance.CheckForCorrectAnswer());
    }
    public void ChangeQuestionTitle(int ID, string title){
        QuestionButton button = null;
        for (int i = 0; i < buttons.Count; i++)
        {
            if(buttons[i].GetComponent<QuestionButton>().GetID() == ID) button = buttons[i].GetComponent<QuestionButton>();
        }

        if(button == null){
            Debug.LogError("No question of ID: " + ID + "Found in the list");
            return;
        }

        button.SetQuestion(title, ID);
    }

    public void SetQuestionButton(int ID){
        //First, (if it exists) lets check for right answers in the old question button, then get the old button to stop displaying
        //AND make sure that the button ID is still part of the quiz. && QuizMaker.instance.GetQuiz().questions.Length > currentButton.GetID()
        if(currentButton != null){
            SetQuestionWarning(QuizMaker.instance.CheckForCorrectAnswer(currentButton.GetID()));
            currentButton.UnSetDisplayAnswer();
        }

        //Next, find the question button with the corosponding ID
        QuestionButton goalButton = null;
        foreach (var item in buttons)
        {
            if(item.GetComponent<QuestionButton>() != null) if(item.GetComponent<QuestionButton>().GetID() == ID) goalButton = item.GetComponent<QuestionButton>();
        }

        if(goalButton != null){
            currentButton = goalButton;
            currentButton.SetDisplayAnswer();
        }
    }

    public void SetQuestionWarning(bool warning){
        if(currentButton == null){
            Debug.LogWarning("No current instance of button found");
            return;
        }

        if(!warning) currentButton.SetWarning();
        else currentButton.UnSetWarning();
    }
}
