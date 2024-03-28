using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PlayerCustomizationManagerRef : MonoBehaviour
{
    [SerializeField] Image[] displayImagesSet;
    #region Singleton
    public static Image[] displayImages;
    void SetDisplaySpritesSingleton(){
        displayImages = new Image[displayImagesSet.Length];
        Array.Copy(displayImagesSet, displayImages, displayImagesSet.Length);
    }
    #endregion
    
    PlayerCustomizationManager cust;
    
    void Awake(){
        SetDisplaySpritesSingleton();
    }

    void Start(){
        cust = PlayerCustomizationManager.instance;
        cust.UpdateDisplaySprites();
    }
    public void ChangeHeadItem(int v){
        cust.ChangeHeadItem(v);
    }
    public void ChangeEyeItem(int v){
        cust.ChangeEyeItem(v);
    }
    public void ChangeMouthItem(int v){
        cust.ChangeMouthItem(v);
    }
}
