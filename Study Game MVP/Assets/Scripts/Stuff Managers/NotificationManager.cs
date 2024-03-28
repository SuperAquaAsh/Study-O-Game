using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    #region Singleton
    public static NotificationManager instance;
    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of NotificationManager found! Deleting gameobject!");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    private void Awake() {
        SetSingleton();
    }

    #endregion

    //We need a refreance to the gameobject to spwan
    [SerializeField] GameObject notObj;
    
    //We need a refreance to the text to change
    [SerializeField] TextMeshProUGUI notText;
    [SerializeField] TextMeshProUGUI butText;

    //We need an event for when the notification is closed
    public event EventHandler OnNotificationClose;

    public void SetNotification(string notificationText, string buttonText, bool error = false){
        notObj.SetActive(true);
        
        notText.text = notificationText;
        butText.text = buttonText;

        if(!error) SFXManager.instance.StartSound(2);
        else SFXManager.instance.StartSound(3);
    }

    public void CloseNotification(){
        notObj.SetActive(false);

        if(OnNotificationClose != null) OnNotificationClose.Invoke(this, EventArgs.Empty);
    }
}
