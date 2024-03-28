using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Custom Items", menuName = "Custom Item")]
public class CustomItems : ScriptableObject
{
    public Sprite[] HeadItems;
    public Sprite[] EyeItems;
    public Sprite[] MouthItems;
}
