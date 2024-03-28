using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//This handles hiding all the player sprites, player customization and fliping the sprite
public class PlayerSprites : NetworkBehaviour
{
    SpriteRenderer[] spriteRenderers;

    [SerializeField]
    Transform visualParent;
    [SerializeField] SpriteRenderer[] customSprites = new SpriteRenderer[3];

    [SerializeField] Player player;

    bool localFlip = false;

    NetworkVariable<bool> isFlipped = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    #region Customization NetworkVariables (byte[] doesn't work)
    NetworkVariable<byte> bodyCustomization = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<byte> eyeCustomization = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<byte> mouthCustomization = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    #endregion 
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }
    void Start(){

        if(IsOwner) {
            return;
        }

        
        bodyCustomization.OnValueChanged += OnCustomChange;
        eyeCustomization.OnValueChanged += OnCustomChange;
        mouthCustomization.OnValueChanged += OnCustomChange;

        UpdateCustomization();
        
        //All code below here is only run if we aren't the owner

        isFlipped.OnValueChanged += OnFlipChange;

        sbyte v = 1;
        if(isFlipped.Value) v = -1;

        visualParent.localScale = new Vector2(1 * v, 1);
    }

    public override void OnNetworkSpawn(){

        if(IsOwner){
            bodyCustomization.Value = PlayerCustomizationManager.instance.customizationItems[0];
            eyeCustomization.Value = PlayerCustomizationManager.instance.customizationItems[1];
            mouthCustomization.Value = PlayerCustomizationManager.instance.customizationItems[2];
        }

        UpdateCustomization();
    }

    int[] DEBUG_ByteToInt(byte[] bytes){
        int[] ints = new int[bytes.Length];
        for (int i = 0; i < ints.Length; i++)
        {
            ints[i] = bytes[i];
        }

        return ints;
    }

    public void ChangeSpriteVisibility(bool visibility)
    {
        foreach (var item in spriteRenderers)
        {
            item.enabled = visibility;
        }
    }

    #region Flipping
    public void FlipSprites(bool flip)
    {
        //If we aren't the owner, then we don't do anything
        if(!IsOwner) return;

        //If we are already in the right direction, then don't do anything
        if(localFlip == flip) return;

        localFlip = flip;

        sbyte v = 1;
        if(flip) v = -1;

        visualParent.localScale = new Vector2(1 * v, 1);
        
        
        isFlipped.Value = localFlip;
    }

    void OnFlipChange(bool previous, bool current){
        //If we are the owner, then don't bother
        if(IsOwner) return;

        sbyte v = 1;
        if(current) v = -1;

        visualParent.localScale = new Vector2(1 * v, 1);
    }

    #endregion

    #region Customization

    void OnCustomChange(byte p, byte c){
        UpdateCustomization();
    }
    
    void UpdateCustomization(){
        //This code is basically copied from UpdateDisplaySprites in PlayerCustomizationManager
        PlayerCustomizationManager.CustomItemSprites itemSprites = PlayerCustomizationManager.GetCustomizationSprites(CustItemsToByteArray());
        customSprites[0].sprite = itemSprites.headSprite;
        customSprites[1].sprite = itemSprites.eyeSprite;
        customSprites[2].sprite = itemSprites.mouthSprite;
    }

    byte[] CustItemsToByteArray(){
        return new byte[]{
            bodyCustomization.Value,
            eyeCustomization.Value,
            mouthCustomization.Value
        };
    }

    #endregion

    public void SpwanAnimationEffect(int id){
        player.playerEffects.SpwanEffect(id);
    }
}
