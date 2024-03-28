using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXRef : MonoBehaviour
{
    public void PlaySound(int id){
        SFXManager.instance.StartSound(id);
    }
}
