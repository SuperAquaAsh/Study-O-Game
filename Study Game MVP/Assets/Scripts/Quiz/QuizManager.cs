using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class QuizManager : MonoBehaviour
{
    [HideInInspector] public static QuizManager instance;

    [SerializeField] QuizRNG quizRNG;
    public Quiz currentQuiz {private set; get;}
    public int quizLength {private set; get;}

    List<ulong> clientsWithQuiz;

    int numOfClientsWithQuiz;
    

    private void Awake() {
        if(instance == null) instance = this;

        if(instance != this){
            Debug.LogWarning("Two Instances of QuizManager found! Killing this gameobject!");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);

        currentQuiz = quizRNG.RandomQuiz();
        quizLength = currentQuiz.questions.Length;
    }

    public async void SetQuizFromLocation(string location){
        currentQuiz = await QuizSaver.instance.LoadQuiz(location);
    }
    public void SetCurrentQuiz(Quiz quiz){
        currentQuiz = quiz;
    }

    #region Question Shtuff
    public Question FetchQuestion(){
        //Just return a random question from the currentQuiz
        return currentQuiz.questions[Random.Range(0, currentQuiz.questions.Length - 1)];
    }
    public Question FetchQuestion(int QuestionNum){
        if(QuestionNum > currentQuiz.questions.Length - 1 || QuestionNum < 0){
            Debug.LogWarning("Question Input is Outside Array size! Returning first question");
            return currentQuiz.questions[0];
        }
        //Just return a random question from the currentQuiz
        return currentQuiz.questions[QuestionNum];
    }
    #endregion

    #region Networked Stuff

    //This is the master function that will be done when All the clients are confirmed to have the right quiz
    public async Task ConfirmQuizzes(){
        //First, we make sure that we reset nessesary variables
        clientsWithQuiz = new List<ulong>();
        numOfClientsWithQuiz = 0;
        
        print("here we go");
        NetworkedQuizManager.instance.ServerCheckClientQuizzes();

        //This just waits until conditions are met (Or until 40 seconds pass)
        float timer = 0f;
        while(numOfClientsWithQuiz < NetworkManager.Singleton.ConnectedClients.Count && timer < 40f){            
            timer += Time.deltaTime;
            await Task.Yield();
        }

        //Now we are done
        print("We are done now. We can start the game!");
    }

    public void AddClientsWithQuiz(ulong clientID){
        if(!clientsWithQuiz.Contains(clientID)){
            //This would break if the clients are ruined
            numOfClientsWithQuiz++;
            clientsWithQuiz.Add(clientID);
        }
    }

    #endregion
}
