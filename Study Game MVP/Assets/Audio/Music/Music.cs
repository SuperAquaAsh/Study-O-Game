using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Music", menuName = "ScriptableObjects/Music")]
public class Music : ScriptableObject
{
    public AudioClip intro;
    public AudioClip loop;
    public AudioClip extraLoop;
    public AudioClip outro;
}
