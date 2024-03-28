using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


//THIS EXISTS BECAUSE: There needs to be the same instance of a networked object in order to communicate data
public class NetworkedQuizManager : NetworkBehaviour
{
    public static NetworkedQuizManager instance;

    ClientRpcParams clientSendParams = new ClientRpcParams{
        Send = new ClientRpcSendParams(),
        Receive = new ClientRpcReceiveParams()
    };

    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of NetworkedQuizManager Found, deleting gameobject! THIS SHOULDN'T HAPPEN");
            Destroy(gameObject);
        }
    }

    public void ServerCheckClientQuizzes(){
        //This is the server sending the quiz code to the clients for them to check if they have it
        CheckQuizCodeClientRpc(QuizManager.instance.currentQuiz.quizCode);
    }

    [ClientRpc]
    void CheckQuizCodeClientRpc(ulong quizCode){
        //This calls another async function simply because ClientRpc's can't be async
        print("I recieved a request to check quiz code: " + quizCode);
        if(!SettingsManager.isWebGl) ClientCheckQuizCode(quizCode);
        else ClientCheckQuizCodeWebGL(quizCode);
    }

    async void ClientCheckQuizCode(ulong quizCode){
        //This exists just because ClientRpc's can't be async
        List<SavedQuiz> savedQuizzes = await QuizSaver.instance.FindAllQuizes();

        SavedQuiz rightquiz = CheckQuizzesForQuizCode(quizCode, savedQuizzes);

        if(rightquiz.saveLocation != "NO"){
            //If we found a good quiz, lets set it
            Quiz quiz = await QuizSaver.instance.LoadQuiz(rightquiz.saveLocation);
            QuizManager.instance.SetCurrentQuiz(quiz);

            //Now we tell the server so we can track it
            print("FOUND SAME QUIZ");
            ClientConfirmQuizServerRpc();
        }else{
            //If not, then we need the new quiz
            print("I need a quiz");
            RequestQuizServerRpc();
        }
    }
    SavedQuiz CheckQuizzesForQuizCode(ulong quizCode, List<SavedQuiz> savedQuizzes){
        foreach (var item in savedQuizzes)
        {
            if(item.quizCode == quizCode) return item;
        }

        return new SavedQuiz{
            saveLocation = "NO",
            quizName = "",
            quizCode = 123,
        };
    }

    void ClientCheckQuizCodeWebGL(ulong quizCode){
        List<Quiz> pastQuizzes = QuizSaver.lastQuizzes;

        Quiz rightQuiz = new Quiz{quizName = null};

        foreach (var item in pastQuizzes)
        {
            if(item.quizCode == quizCode) rightQuiz = item;
        }

        if(rightQuiz.quizName != null){
            QuizManager.instance.SetCurrentQuiz(rightQuiz);

            //Now we tell the server so we can track it
            print("FOUND SAME QUIZ! IN WEBGL!");
            ClientConfirmQuizServerRpc();
        }else{
            //If not, then we need the new quiz
            print("I need a quiz");
            RequestQuizServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ClientConfirmQuizServerRpc(ServerRpcParams rpcParams = default){
        //This is for the sever to count the people with a quiz
        QuizManager.instance.AddClientsWithQuiz(rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestQuizServerRpc(ServerRpcParams rpcParams = default){
        //This is for the SERVER to send the quiz they have to all the CLIENTS
        if(!IsServer) return;
        print("I'm bout to send a quiz");

        clientSendParams.Send.TargetClientIds = new List<ulong>{
            rpcParams.Receive.SenderClientId
        };

        //This SHOULD only send the quiz to the client that requested it
        ReceiveQuizClientRpc(QuizManager.instance.currentQuiz, clientSendParams);
    }

    [ClientRpc]
    void ReceiveQuizClientRpc(Quiz sentQuiz, ClientRpcParams rpcParams = default){
        if(IsServer) return;
        print("I GOT A QUIZ!");
        QuizManager.instance.SetCurrentQuiz(sentQuiz);

        //We download the quiz
        if(!SettingsManager.isWebGl) QuizSaver.instance.SaveQuiz(sentQuiz);
        else QuizSaver.instance.AddQuizWebGL(sentQuiz);

        //Now we tell the server so it can track
        ClientConfirmQuizServerRpc();
    }


    public NetworkManager GetNetworkManager(){
        return NetworkManager.Singleton;
    }
}
