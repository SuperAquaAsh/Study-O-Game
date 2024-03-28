using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This allows Hiding Manager to get access to all the hiding spots in dynamicaly loaded scenes
/// </summary>
public class HidingSet : MonoBehaviour
{
    [SerializeField] List<Hiding> hidingSpots;
    // Start is called before the first frame update
    void Start()
    {
        if(hidingSpots.Count == 0){
            //If you are seeing an error, it makes sense
            hidingSpots = GetComponentsInChildren<Hiding>().ToList();
        }

        print(hidingSpots);
        print(HidingManager.instance);
        HidingManager.instance.SetHidingSpots(hidingSpots);
    }
}
