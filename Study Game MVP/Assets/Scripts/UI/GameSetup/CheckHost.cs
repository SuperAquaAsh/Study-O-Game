using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckHost : MonoBehaviour
{
    [SerializeField] SetUpDirection setUp;
    public void CheckQuizzes(){
        if(QuizSaver.instance.IsMoreThanOneQuiz()){
            setUp.CheckForMaxConnect();
        }else{
            NotificationManager.instance.OnNotificationClose += ReturnToMenu;
            NotificationManager.instance.SetNotification("You don't have enough quizzes to host! Create some quizzes or join a game instead. :)", "Okay", true);
        }
    }

    void ReturnToMenu(object s, EventArgs e){
        SceneManager.LoadScene(0);
        NotificationManager.instance.OnNotificationClose -= ReturnToMenu;
    }
}
