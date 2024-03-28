using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//This just allows us to refrence values from the settings
//It also allows us to always refrence the settings manager
public class SettingsMenu : MonoBehaviour
{
    #region Singleton
    public static SettingsMenu instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of HasSetSettings found. Destroying this gameobject");
            Destroy(gameObject);
            return;
        }
    }

    private void Awake() {
        SetSingleton();
    }

    #endregion
    public bool hasSet;

    public TMP_InputField nicknameText;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    
    public Slider volumeSlider;
    public Slider masterVolumeSlider;

    public void SetNickname(string v){
        SettingsManager.instance.SetNickname(v);
    }
    public void SetFullscreen(bool v){
        SettingsManager.instance.SetFullscreen(v);
    }
    public void SetVolume(float v){
        SettingsManager.instance.SetVolume(v);
    }
    public void SetMasterVolume(float v){
        SettingsManager.instance.SetMasterVolume(v);
    }
    public void SetResolution(int i){
        SettingsManager.instance.SetResolution(i);
    }

    public void SaveSettings(){
        SettingsManager.instance.SaveSettingsButton();
    }
}
