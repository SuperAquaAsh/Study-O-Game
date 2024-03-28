using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [Header("Follow")]
    public GameObject followObject;
    [SerializeField] bool autoOffset;
    public Vector3 offset;

    [Header("Axis")]
    [SerializeField] bool followX = true;
    [SerializeField] bool followY = true;
    [SerializeField] bool followZ = true;

    [SerializeField] bool followScale;


    
    // Start is called before the first frame update
    void Start()
    {
        if(followObject == null){
            Debug.LogWarning("No Object for me to Follow! Please Assign an Object!");
            return;
        }
        if(autoOffset){
            offset += transform.position - followObject.transform.position;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Check if the object exists yet
        if(followObject == null){
            return;
        }

        //Check for follow Scale
        if(followScale){
            transform.localScale = followObject.transform.lossyScale;
        }

        //Just check if it follows all the axis to save a bit of work
        if(followX && followY && followZ)
        {
            transform.position = followObject.transform.position + offset;
            return;
        }

        //follow the idididual axis
        if(followX)
        {
            transform.position = new Vector3(followObject.transform.position.x + offset.x, 0f, 0f);
        }
        if(followY)
        {
            transform.position = new Vector3(0f, followObject.transform.position.y + offset.y, 0f);
        }
        if(followZ)
        {
            transform.position = new Vector3(0f, 0f, followObject.transform.position.z + offset.z);
        }
    }
}
