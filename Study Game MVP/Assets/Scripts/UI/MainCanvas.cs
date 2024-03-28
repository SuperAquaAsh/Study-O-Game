using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//This literattly just exists so other objects know where
//the main canvas is (and some of it's attached components)
public class MainCanvas : MonoBehaviour
{
    #region Singleton
    public static MainCanvas instance;
    #endregion

    public GameObject QuestionDisplay;
    public GameObject GameSetup;
    public GameObject clientGameSetup;
    public TextMeshProUGUI joinCodeText;

    private void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of MainCanvas found! Deleting this gameobject");
            Destroy(gameObject);
        }
    }

    public void LeaveLobby(){
        //We will also start the right music
        MusicManager.instance.SetMusic(0, 0.5f);

        GameManager.instance.LeaveLobby();
    }
}
