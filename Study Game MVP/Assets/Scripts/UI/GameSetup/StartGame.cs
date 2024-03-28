using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is just so a button can start the game, and to copy join code
public class StartGame : MonoBehaviour
{
    public void StartTheGame(){
        GameManager.instance.StartGame();
    }

    public void CopyJoinCode(){
       if(GameManager.instance.isRelay) GUIUtility.systemCopyBuffer = GameManager.instance.joinCode;
    }
}
