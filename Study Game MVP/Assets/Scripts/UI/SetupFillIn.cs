using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//This fills in text so it makes sense in contexts
public class SetupFillIn : MonoBehaviour
{
    //These are filled in with "IP" or "Join Code" depending if it is relay
    [SerializeField] TextMeshProUGUI[] connectionTypeTexts;
    string[] connectionTypeOriginal;

    //These are filled in with "Relay" or "Ports" depending on the connection type
    [SerializeField] TextMeshProUGUI[] connectionFillInTexts;
    string[] connectionFillInOriginal;
    
    //These are filled in with "Host" or "Join" depending on what we are doing
    [SerializeField] TextMeshProUGUI[] hostFillInTexts;
    string[] hostFillInOriginal;

    //These are filled in with the IP or Join Cod depending on if it is relay or not
    [SerializeField] TextMeshProUGUI[] dataFillInTexts;
    string[] dataFillInOriginal;

    bool setOrignal = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void UpdateStuff()
    {
        //Reset the data or record it. Depends if you did it or not
        if(setOrignal) ResetOrignalData();
        else SetOriginalData();

        if(connectionTypeTexts != null) FillInConnectionType();
        if(connectionFillInTexts != null) FillInConnection();
        if(hostFillInTexts != null) FillInHost();
        if(dataFillInTexts != null) FillInData();
    }


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

    #region FillIn Functions
    void FillInConnectionType(){
        //Looks for # to replace
        string replace;
        if(GameManager.instance.isRelay) replace = "Join Code";
        else replace = "IP";

        foreach (var item in connectionTypeTexts)
        {
            item.text = ReplaceString(item.text, "#", replace);
        }
    }
    void FillInConnection(){
        //Looks for @ to replace
        string replace;
        if(GameManager.instance.isRelay) replace = "Relay";
        else replace = "Ports";

        foreach (var item in connectionFillInTexts)
        {
            item.text = ReplaceString(item.text, "@", replace);
        }
    }
    void FillInHost(){
        //Looks for @ to replace
        string replace;
        if(GameManager.instance.isHost) replace = "Host";
        else replace = "Join";

        foreach (var item in hostFillInTexts)
        {
            item.text = ReplaceString(item.text, "@", replace);
        }
    }
    void FillInData(){
        //Looks for @ to replace
        string replace;
        if(GameManager.instance.isRelay) replace = GameManager.instance.joinCode;
        else replace = GameManager.instance.address;

        foreach (var item in dataFillInTexts)
        {
            item.text = ReplaceString(item.text, "@", replace);
        }
    }

    #endregion

    //This stores the orignal texts to ensure we can rechange them later
    void SetOriginalData(){
        //If we already did this, then no need!
        if(setOrignal){
            return;
        }

        //Transfer data from each array of textMeshes to the corosponding string array
        TransferTextToString(connectionTypeTexts, ref connectionTypeOriginal);
        TransferTextToString(connectionFillInTexts, ref connectionFillInOriginal);
        TransferTextToString(hostFillInTexts, ref hostFillInOriginal);
        TransferTextToString(dataFillInTexts, ref dataFillInOriginal);

        //Remember that we already did this so we don't do it twice
        setOrignal = true;
    }

    void TransferTextToString(TextMeshProUGUI[] orignalTexts, ref string[] fillInString){
        //Creates an array of the right length
        fillInString = new string[orignalTexts.Length];

        //populates that array with the text data from the TextMeshProUGUI
        for (int i = 0; i < orignalTexts.Length; i++)
        {
            fillInString[i] = orignalTexts[i].text;
        }
    }

    //This actually changes the texts back to there orginal
    void ResetOrignalData(){
        //Transfer data from each array of textMeshes to the corosponding string array
        PutStringIntoTextMesh(connectionTypeOriginal, ref connectionTypeTexts);
        PutStringIntoTextMesh(connectionFillInOriginal, ref connectionFillInTexts);
        PutStringIntoTextMesh(hostFillInOriginal, ref hostFillInTexts);
        PutStringIntoTextMesh(dataFillInOriginal, ref dataFillInTexts);
    }

    //This takes in strings and TextMeshProUGUIs and sets the textMeshes to their corosponding strings
    void PutStringIntoTextMesh(string[] strings, ref TextMeshProUGUI[] texts){
        if(strings.Length != texts.Length){
            Debug.LogWarning("String and Text array length does not match, ensure the array wasn't edited");
        }

        for (int i = 0; i < strings.Length; i++)
        {
            texts[i].text = strings[i];
        }
    }

}
