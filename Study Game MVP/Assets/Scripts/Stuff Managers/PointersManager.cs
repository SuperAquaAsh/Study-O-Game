using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointersManager : MonoBehaviour
{
    #region Singleton
    public static PointersManager instance;
    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of PointersManager found, deleting this gameobject");
            Destroy(gameObject);
            return;
        }
    }
    #endregion
    
    [SerializeField] GameObject pointerObj;

    [HideInInspector] public Dictionary<ulong, OffscreenPointer> playerPointerDictionary = new Dictionary<ulong, OffscreenPointer>();

    void Awake(){
        SetSingleton();
    }

    public void SpwanPointer(Player player){
        OffscreenPointer pointer = Instantiate(pointerObj, transform).GetComponent<OffscreenPointer>();
        pointer.SetFollow(player);

        playerPointerDictionary.Add(player.OwnerClientId, pointer);
    }

    public void SpwanPointer(GameObject obj){
        GameObject go = Instantiate(pointerObj, transform);
        go.GetComponent<OffscreenPointer>().SetFollow(obj);
    }

    public void DeletePlayerPointer(ulong id){
        playerPointerDictionary[id].DeletPointer();

        playerPointerDictionary.Remove(id);
    }
}
