using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This just allows us to refrence SettingsManager when it changes scenes
public class SetManRef : MonoBehaviour
{
    public void LoadSettings(){
        SettingsManager.instance.LoadSettingsButton();
    }
}
