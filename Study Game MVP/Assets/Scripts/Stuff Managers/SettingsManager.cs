using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.Audio;

#region Setttings Struct

[Serializable]
public struct Settings{
    public string nickname;
    public float volume;
    public float masterVolume;
    public long lastNotificationReadDate;
}

#endregion


//This should ALSO handle saving data to the disk
public class SettingsManager : MonoBehaviour
{
    #region Singleton
    public static SettingsManager instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of SettingsManager found. Destroying this gameobject");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    #endregion


    //FIX THIS!!! It shouldn't be public to everyone
    public Settings currentSettings;

    [SerializeField] AudioMixer masterMixer;

    public static readonly string SETTINGS_SAVE_FOLDER = "/GameSettings/";
    public static readonly string SETTINGS_FILENAME = "StudySettings.set";

    #region WebGl Variables
    public static bool isWebGl = false;

    const string nicknameKey = "playerNickname";
    const string volumeKey = "volume";
    const string masterVolumeKey = "masterVolume";
    const string notificationKey = "lastNotificationReadDate";
    #endregion

    public static readonly Settings defaultSettings = new Settings{
        nickname = "player",
        volume = 0.8f,
        masterVolume = 1f,
        lastNotificationReadDate = 0
    };

    private void Awake() {
        SetSingleton();
        if(Application.platform == RuntimePlatform.WebGLPlayer) isWebGl = true;
    }

    private void Start() {
        MusicManager.instance.SetVolume(0.5f);
        LoadSettings(false);
    }

    #region Load/Save Data
    public void LoadSettingsButton(bool initalize = true){
        if(!isWebGl) LoadSettings(initalize);
        else LoadWebGl(initalize);
    }
    public async void LoadSettings(bool initalize = true){
        if(isWebGl){
            LoadWebGl(initalize);
            return;
        }

        //We want to check if the folder exists. We create the folder if it doesn't
        if(!Directory.Exists(Application.persistentDataPath + SETTINGS_SAVE_FOLDER)){
            Directory.CreateDirectory(Application.persistentDataPath + SETTINGS_SAVE_FOLDER);
            while(!IsAccessible(new DirectoryInfo(Application.persistentDataPath + SETTINGS_SAVE_FOLDER))) await Task.Yield();
        }
        

        //We check if the file exists
        //We create it with default values if it doesn't
        if(!File.Exists(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME)){
            //Code here NOT COPIED, but inspired by: https://www.c-sharpcorner.com/UploadFile/mahesh/create-a-text-file-in-C-Sharp/
            using(StreamWriter sw = File.CreateText(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME)){
                await sw.WriteAsync(JsonUtility.ToJson(defaultSettings));
            }
        }

        //We get the data from the file
        try{
            string data = await File.ReadAllTextAsync(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME);
            Settings settings = JsonUtility.FromJson<Settings>(data);
            currentSettings = settings;
        }catch(Exception e){
            Debug.LogWarning("While reading the settings file, we got error: " + e);
            //We also reset the setting values for safety
            await File.WriteAllTextAsync(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME, JsonUtility.ToJson(defaultSettings));
        }
        print("settings are loaded! at location: " + Application.persistentDataPath);

        //Set the music now that it's loaded
        MusicManager.instance.SetVolume(currentSettings.volume);

        //Set the masterVolume now that its loaded
        masterMixer.SetFloat("Master_Volume", currentSettings.masterVolume);

        if(initalize) InitalizeSettings();
    }

    public void SaveSettingsButton(){
        if(!isWebGl) SaveSettings();
        else SaveWebGl();
    }

    public async Task SaveSettings(){

        //We want to check if the folder exisits. We create the folder if it doesn't
        if(!Directory.Exists(Application.persistentDataPath + SETTINGS_SAVE_FOLDER)){
            Directory.CreateDirectory(Application.persistentDataPath + SETTINGS_SAVE_FOLDER);
        }

        //We want to check if the file exists
        //We create it with default values if it doesn't
        if(!File.Exists(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME)){
            File.Create(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME);
            await File.WriteAllTextAsync(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME, JsonUtility.ToJson(new Settings()));
        }

        //We write to that file
        string data = JsonUtility.ToJson(currentSettings);
        await File.WriteAllTextAsync(Application.persistentDataPath + SETTINGS_SAVE_FOLDER + SETTINGS_FILENAME, data);
        print("settings saved!");
    }

    //Code from: https://stackoverflow.com/questions/11709862/check-if-directory-is-accessible-in-c
    bool IsAccessible(DirectoryInfo realpath)
    {
        try
        {
            //if GetDirectories works then is accessible
            realpath.GetDirectories();       
            return true;
        }
        catch (Exception)
        {
            //if exception is not accesible
            return false;
        }
    }

    #endregion

    //This is public so we can call it when we activate the menu

    #region Load/Save WebGl Data

    void LoadWebGl(bool initalize){        

        //We check if the key exists, and create it if it doesn't
        //Add ORs here for more data stuff
        if(!PlayerPrefs.HasKey(nicknameKey) || !PlayerPrefs.HasKey(notificationKey) || !PlayerPrefs.HasKey(volumeKey) || !PlayerPrefs.HasKey(masterVolumeKey)){
            WriteToWebGl(defaultSettings);
        }

        //We get the data from the file
        currentSettings.nickname = PlayerPrefs.GetString(nicknameKey);
        currentSettings.lastNotificationReadDate = long.Parse(PlayerPrefs.GetString(notificationKey));
        currentSettings.volume = float.Parse(PlayerPrefs.GetString(volumeKey));
        currentSettings.masterVolume = PlayerPrefs.GetFloat(masterVolumeKey);

        MusicManager.instance.SetVolume(currentSettings.volume);

        if(initalize) InitalizeSettings();
    }

    void SaveWebGl(){
        WriteToWebGl(currentSettings);
    }

    //UPDATE THIS AS THE SETTINGS GET BIGGER
    void WriteToWebGl(Settings settings){
        PlayerPrefs.SetString(nicknameKey, settings.nickname);
        PlayerPrefs.SetString(notificationKey, settings.lastNotificationReadDate.ToString());
        PlayerPrefs.SetString(volumeKey, settings.volume.ToString());
        PlayerPrefs.SetFloat(masterVolumeKey, settings.masterVolume);
    }

    #endregion

    public void InitalizeSettings(){
        if(SettingsMenu.instance.hasSet) return;
        SettingsMenu.instance.hasSet = true;
        SettingsMenu.instance.nicknameText.text = currentSettings.nickname;
        if(!isWebGl){
            //We get a list of the resolutions
            List<string> resolutions = new List<string>();
            Resolution[] resArray = Screen.resolutions;
            int rightIndex = 0;
            for (int i = 0; i < resArray.Length; i++)
            {
                int x = resArray[i].width;
                int y = resArray[i].height;
                resolutions.Add(x + " x " + y + " (" + string.Format("{0}:{1}",x/GCD(x,y), y/GCD(x,y)) + ")");
                if(x == Screen.currentResolution.width && y == Screen.currentResolution.height) rightIndex = i;
            }

            SettingsMenu.instance.resolutionDropdown.ClearOptions();
            SettingsMenu.instance.resolutionDropdown.AddOptions(resolutions);
            SettingsMenu.instance.resolutionDropdown.SetValueWithoutNotify(rightIndex);
        }

        SettingsMenu.instance.fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

        SettingsMenu.instance.volumeSlider.SetValueWithoutNotify(currentSettings.volume);

        SettingsMenu.instance.masterVolumeSlider.SetValueWithoutNotify(currentSettings.masterVolume);
    }

    //CODE HERE FROM: https://stackoverflow.com/questions/10070296/c-sharp-how-to-calculate-aspect-ratio
    static int GCD(int a, int b)
    {
        int Remainder;

        while( b != 0 )
        {
            Remainder = a % b;
            a = b;
            b = Remainder;
        }

        return a;
    }

    #region Set/Get Settings values Functions

    public void SetNickname(string v){
        currentSettings.nickname = v;
    }
    public void SetFullscreen(bool v){
        Screen.fullScreen = v;
    }
    public void SetVolume(float v){
        currentSettings.volume = v;
        MusicManager.instance.SetVolume(currentSettings.volume);
    }
    public void SetMasterVolume(float v){
        currentSettings.masterVolume = v;
        float calculatedVolume = Mathf.Lerp(-80f, 10f, currentSettings.masterVolume);
        masterMixer.SetFloat("Master_Volume", calculatedVolume);
    }
    public void SetResolution(int i){
        Resolution resolution = Screen.resolutions[i];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetNotificationReadDate(long l){
        currentSettings.lastNotificationReadDate = l;
    }
    public long GetNotificationReadDate(){
        return currentSettings.lastNotificationReadDate;
    }

    #endregion
}
