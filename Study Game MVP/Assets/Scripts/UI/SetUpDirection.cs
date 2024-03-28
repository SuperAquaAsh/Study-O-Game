using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This basically skips the Data part if someone is hosting
//AND lets relay hosts select the Max Connections
public class SetUpDirection : MonoBehaviour
{
    [SerializeField] GameObject DataMenu;
    [SerializeField] GameObject FinalMenu;
    [SerializeField] GameObject MaxConnectMenu;
    public void CheckForData(){
        if(GameManager.instance.isHost){
            FinalMenu.SetActive(true);
        }else{
            DataMenu.SetActive(true);
        }
    }

    public void CheckForMaxConnect(){
        if(GameManager.instance.isHost && GameManager.instance.isRelay){
            MaxConnectMenu.SetActive(true);
        }else{
            FinalMenu.SetActive(true);
        }
    }
}
