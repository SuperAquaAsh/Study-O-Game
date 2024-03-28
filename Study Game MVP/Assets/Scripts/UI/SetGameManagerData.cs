using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//This exists so we can ALWAYS reference the game Manager
public class SetGameManagerData : MonoBehaviour
{
    public void SetConnectionType(bool relay){
        GameManager.instance.SetConnectionType(relay);
    }

    public void SetHost(bool host){
        GameManager.instance.SetHost(host);
    }

    public void SetConnectionData(string data){
        GameManager.instance.SetConnectionData(data);
    }
    public void SetConnectionNumber(int num){
        GameManager.instance.SetConnectionNumber(num);
    }

    public void InitializeGame(){
        GameManager.instance.InitializeGame();
    }

    public void QuitApplication(){
        GameManager.instance.QuitApplication();
    }

}
