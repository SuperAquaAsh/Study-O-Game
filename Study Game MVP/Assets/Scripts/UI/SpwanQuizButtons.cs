using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This spwans in the Quiz Buttons so the host can select the quiz
public class SpwanQuizButtons : MonoBehaviour
{
    [SerializeField] Transform selectorGO;
    [SerializeField] GameObject quizButtonPrefab;
    [SerializeField] GameObject nextMenu;

    //A lot of this code is copied from the QuizMaker version of this
    public async void SpwanButtons(){
        //First, lets get a list of all the buttons
        List<SavedQuiz> quizzes = await QuizSaver.instance.FindAllQuizes();
        if(quizzes == null){
            //If nothing is on disk then don't do anything
            Debug.LogWarning("No quizzes Found on Disk");
            return;
        }
        
        //Before we start spwaning, we should delete all the old children (If any)
        DeleteChildren(selectorGO);

        //Next, we spwan the buttons in under the selectorGO
        QuizButton button = null;
        foreach (var item in quizzes)
        {
            button = Instantiate(quizButtonPrefab, selectorGO.transform).GetComponent<QuizButton>();
            button.SetQuizData(item.saveLocation, item.quizName);
            button.SetMenuData(gameObject, nextMenu);
        }
    }

    void DeleteChildren(Transform trans){
        if(trans.childCount < 1){
            Debug.LogWarning("No chlidren to delete on Transform: " + trans.name);
            return;
        }
        for (int i = 0; i < trans.childCount; i++)
        {
            Destroy(trans.GetChild(i).gameObject);
        }
    }
}
