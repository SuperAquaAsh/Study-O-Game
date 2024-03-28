using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//This script ensures that the right data is shown just before you start a game
public class FinalizeSetupMenu : MonoBehaviour
{
    //This shows "Ports" or "Relay"
    [SerializeField] TextMeshProUGUI connectionTypeTextMesh;
    string origanlConnectionType;
    //This shows "Hosting" or "Joining"
    [SerializeField] TextMeshProUGUI hostTextMesh;
    string orignalHost;
    //This shows the IP or Join Code.
    [SerializeField] TextMeshProUGUI dataTypeTextMesh;
    string orginaldataType;

    //This shows the max connections if hosting
    [SerializeField] TextMeshProUGUI maxTypeTextMesh;
    string orignalMaxType;

    //This keeps track if we have stored the stings yet
    bool setOrignal = false;

    public void FillInFinalize(){
        //We set the orignal data if we haven't yet, and reset it if we have
        if(setOrignal) ResetOrignalData();
        else SetOriginalData();
        
        string replace;
        GameManager manager = GameManager.instance;

        //Set all the data to what it should be

        //---------------------------------

        if(manager.isRelay) replace = "Relay";
        else replace = "Ports";

        connectionTypeTextMesh.text = ReplaceString(connectionTypeTextMesh.text, "@", replace);

        //---------------------------------

        if(manager.isHost) replace = "Hosting";
        else replace = "Joining";

        hostTextMesh.text = ReplaceString(hostTextMesh.text, "@", replace);

        //---------------------------------

        replace = manager.GetConnectionData();

        dataTypeTextMesh.text = ReplaceString(dataTypeTextMesh.text, "@", replace);

        //---------------------------------

        if(manager.isRelay) replace = "Join Code";
        else replace = "IP";

        dataTypeTextMesh.text = ReplaceString(dataTypeTextMesh.text, "#", replace);

        //---------------------------------
        
        replace = manager.maxConnections.ToString();

        maxTypeTextMesh.text = ReplaceString(maxTypeTextMesh.text, "@", replace);

        //---------------------------------

        //Now we hide data depending if we are the host or not

        if(manager.isHost){
            dataTypeTextMesh.gameObject.SetActive(false);
            maxTypeTextMesh.gameObject.SetActive(true);
        }else{
            dataTypeTextMesh.gameObject.SetActive(true);
            maxTypeTextMesh.gameObject.SetActive(false);
        }
        if(!manager.isRelay) maxTypeTextMesh.gameObject.SetActive(false);

    }

    #region Orignaly from SetupFillIn (Setting Orignal Data)
    
    //This stores the orignal texts to ensure we can rechange them later
    void SetOriginalData(){
        //If we already did this, then no need!
        if(setOrignal){
            return;
        }

        //Transfer data from each array of textMeshes to the corosponding string array
        TransferTextToString(connectionTypeTextMesh, ref origanlConnectionType);
        TransferTextToString(hostTextMesh, ref orignalHost);
        TransferTextToString(dataTypeTextMesh, ref orginaldataType);
        TransferTextToString(maxTypeTextMesh, ref orignalMaxType);

        //Remember that we already did this so we don't do it twice
        setOrignal = true;
    }

    void TransferTextToString(TextMeshProUGUI orignalText, ref string fillInString){
        fillInString = orignalText.text;
    }

    //This actually changes the texts back to there orginal
    void ResetOrignalData(){
        //Transfer data from each array of textMeshes to the corosponding string array
        PutStringIntoTextMesh(origanlConnectionType, ref connectionTypeTextMesh);
        PutStringIntoTextMesh(orignalHost, ref hostTextMesh);
        PutStringIntoTextMesh(orginaldataType, ref dataTypeTextMesh);
        PutStringIntoTextMesh(orignalMaxType, ref maxTypeTextMesh);
    }

    //This takes in strings and TextMeshProUGUIs and sets the textMeshes to their corosponding strings
    void PutStringIntoTextMesh(string replace, ref TextMeshProUGUI text){
        text.text = replace;
    }

    #endregion

    //This is from SetupFillIn
    string ReplaceString(string input, string targetChar, string replace){
        int changePoint = -1;
        for (int i = 0; i < input.Length; i++)
        {
            if (string.Equals(input[i].ToString(), targetChar)){
                changePoint = i;
            }
        }
        
        if(changePoint == -1){
            Debug.LogWarning("No Point to replace found in string");
            return input;
        }
        if(replace == "" || replace == null){
            input = input.Remove(changePoint, 1);
            return input;
        }

        input = input.Remove(changePoint, 1);
        input = input.Insert(changePoint, replace);

        return input;
    }

    private void OnEnable() {
        FillInFinalize();
    }
}
