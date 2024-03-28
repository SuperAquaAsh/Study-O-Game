using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizationManager : MonoBehaviour
{
    #region Singleton
    public static PlayerCustomizationManager instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
            customItems = setCustomItems;
            DontDestroyOnLoad(gameObject);
        }else{
            Debug.LogWarning("Two instaces of PlayerCustomizationManager found, destroying this object");
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField] CustomItems setCustomItems;

    public static CustomItems customItems;

    public byte[] customizationItems {get; private set;} = new byte[3];

    
    public EventHandler<EventArgs> OnCustomizationChanged;


    public struct CustomItemSprites{
        public Sprite headSprite;
        public Sprite eyeSprite;
        public Sprite mouthSprite;
    }

    void Awake(){
        SetSingleton();
    }

    public void ChangeHeadItem(int v){
        customizationItems[0] = (byte)(customizationItems[0] + v);
        if(customizationItems[0] == 255) customizationItems[0] = (byte)(customItems.HeadItems.Length - 1);
        if(customizationItems[0] > customItems.HeadItems.Length - 1) customizationItems[0] -= (byte)customItems.HeadItems.Length;
        
        if(OnCustomizationChanged != null) OnCustomizationChanged.Invoke(this, EventArgs.Empty);

        UpdateDisplaySprites();
    }
    public void ChangeEyeItem(int v){
        customizationItems[1] = (byte)(customizationItems[1] + v);
        if(customizationItems[1] == 255) customizationItems[1] = (byte)(customItems.EyeItems.Length - 1);
        if(customizationItems[1] > customItems.EyeItems.Length - 1) customizationItems[1] -= (byte)customItems.EyeItems.Length;
        
        if(OnCustomizationChanged != null) OnCustomizationChanged.Invoke(this, EventArgs.Empty);

        UpdateDisplaySprites();
    }
    public void ChangeMouthItem(int v){
        customizationItems[2] = (byte)(customizationItems[2] + v);
        if(customizationItems[2] == 255) customizationItems[2] = (byte)(customItems.MouthItems.Length - 1);
        if(customizationItems[2] > customItems.MouthItems.Length - 1) customizationItems[2] -= (byte)customItems.MouthItems.Length;
        
        if(OnCustomizationChanged != null) OnCustomizationChanged.Invoke(this, EventArgs.Empty);

        UpdateDisplaySprites();
    }

    public void UpdateDisplaySprites(){
        CustomItemSprites itemSprites = GetCustomizationSprites(customizationItems);
        PlayerCustomizationManagerRef.displayImages[0].sprite = itemSprites.headSprite;
        PlayerCustomizationManagerRef.displayImages[1].sprite = itemSprites.eyeSprite;
        PlayerCustomizationManagerRef.displayImages[2].sprite = itemSprites.mouthSprite;

        //set the image transparent if there is nothing
        UnityEngine.UI.Image image = PlayerCustomizationManagerRef.displayImages[0];
        if(image.sprite == null) image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        else image.color = new Color(image.color.r, image.color.g, image.color.b, 255);

        image = PlayerCustomizationManagerRef.displayImages[1];
        if(image.sprite == null) image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        else image.color = new Color(image.color.r, image.color.g, image.color.b, 255);

        image = PlayerCustomizationManagerRef.displayImages[2];
        if(image.sprite == null) image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        else image.color = new Color(image.color.r, image.color.g, image.color.b, 255);
    }

    #region Translate (Add code for byte[] to ID and visa versa)

    public static CustomItemSprites GetCustomizationSprites(byte[] items){
        return new CustomItemSprites(){
            headSprite = customItems.HeadItems[items[0]],
            eyeSprite = customItems.EyeItems[items[1]],
            mouthSprite = customItems.MouthItems[items[2]]
        };
    }

    #endregion
}
