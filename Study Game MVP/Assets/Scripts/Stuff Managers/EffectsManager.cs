using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

//"Inspired" by https://www.youtube.com/watch?v=9O7uqbEe-xc
public class EffectsManager : NetworkBehaviour
{
    //Create a static list of posible effects (and a way to set them)
    [SerializeField] ObjectPool[] effects;

    static GameObject poolsParent;

    static ObjectPool[] objectPools;

    #region Singleton
    public static EffectsManager instance;

    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instances of EffectsManager found! Destorying this object!");
            Destroy(gameObject);
        }
    }
    #endregion

    void Awake(){
        SetSingleton();

        if(instance != this) return;

        print("Spwaned in object");
        poolsParent = new GameObject(){
            name = "Object Pools"
        };

        objectPools = new ObjectPool[effects.Length];

        for (int i = 0; i < effects.Length; i++)
        {
            //We create the a ObjectPool for each effect
            objectPools[i] = effects[i];
            objectPools[i].poolId = i;
            objectPools[i].parentObject = new GameObject(){
                name = objectPools[i].startObject.name + " | Pooled Parent"
            };
            objectPools[i].parentObject.transform.parent = poolsParent.transform;

            for(int v = 0; v < objectPools[i].startSize; v++){
                GameObject game = Instantiate(objectPools[i].startObject, objectPools[i].parentObject.transform);
                game.SetActive(false);
                objectPools[i].pooledObjects.Add(game);
            }
        }

        //We do a quick check to make sure none of the names of the objects match

        for (int i = 0; i < objectPools.Length; i++)
        {
            for (int v = 0; v < objectPools.Length; v++)
            {
                if(i != v){
                    if(objectPools[i].startObject.name == objectPools[v].startObject.name){
                        Debug.LogError("Two names inside the object pools match. This will cause errors! Please rename one of your pooled objects from: " + objectPools[v].startObject.name);
                    }
                }
            }
        }

    }

    #region Spwan Object

    #region Networked Spwan
    public GameObject SpwanObjectsNetworked(int objId, Vector3 location, Quaternion rotation){
        //tell the server to spwan this object on all clients
        ClientRequestEffectSpwanServerRpc(objId, location, rotation);

        return SpwanObjectFromPool(objId, location, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    void ClientRequestEffectSpwanServerRpc(int objId, Vector3 location, Quaternion rotation, ServerRpcParams par = default){
        //Create a ClientRpcParams that removes the senders ID
        List<ulong> targetIds = NetworkManager.ConnectedClientsIds.ToList();
        targetIds.Remove(par.Receive.SenderClientId);

        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{
                TargetClientIds = targetIds
            }
        };

        ReceiveEffectToSpwanClientRpc(objId, location, rotation, clientRpcParams);
    }


    [ClientRpc]
    void ReceiveEffectToSpwanClientRpc(int objId, Vector3 location, Quaternion rotation, ClientRpcParams par = default){
        SpwanObjectFromPool(objId, location, rotation);
    }


    #endregion
    //Create a static function to spwan in the objects
    public static GameObject SpwanObjectFromPool(int objId, Vector3 location, Quaternion rotation){
        if(rotation == Quaternion.identity){
            rotation = Quaternion.LookRotation(Vector3.right, -Vector3.forward);
        }
        
        //We want to locate the object that we want to spwan in
        if(objId >= objectPools.Length) {
            Debug.LogError("The Object ID you are requesting is outside of the pools range");
            return null;
        }

        //fetch the right pool
        ObjectPool rightPool = objectPools[objId];

        GameObject spwanObject = null;

        //look in the list for any objects
        if(rightPool.pooledObjects.Count > 0){

            spwanObject = rightPool.pooledObjects[0];
            rightPool.pooledObjects.Remove(spwanObject);
        }

        if(spwanObject == null) {
            //if we didn't find anything we need to instaniate something new
            spwanObject = Instantiate(rightPool.startObject);
        }

        spwanObject.SetActive(true);
        spwanObject.transform.position = location;
        spwanObject.transform.rotation = rotation;

        return spwanObject;
    }
    public static GameObject SpwanObjectFromPool(GameObject obj, Vector3 location, Quaternion rotation){
        //We want to locate the object that we want to spwan in
        ObjectPool rightPool = GetPoolFromGameObject(obj);
        if(rightPool == null) {
            Debug.LogError("The Object you are requesting is not part of any pools. Please add this object to the pools beforehand");
            return null;
        }

        GameObject spwanObject = null;

        //look in the list for any objects
        if(rightPool.pooledObjects.Count > 0){
            spwanObject = rightPool.pooledObjects[0];
            rightPool.pooledObjects.Remove(spwanObject);
        }

        if(spwanObject == null) {
            //if we didn't find anything we need to instaniate something new
            spwanObject = Instantiate(rightPool.startObject);
        }

        spwanObject.SetActive(true);
        spwanObject.transform.position = location;
        spwanObject.transform.rotation = rotation;

        print(spwanObject.transform.rotation * Vector3.forward);

        return spwanObject;
    }

    #endregion


    #region Return Object
    //Create a static function to return the objects
    public static void ReturnObjectToPool(GameObject returnObj, int poolId){
        if(poolId >= objectPools.Length) {
            Debug.LogError("The Object ID you are requesting is outside of the pools range");
            return;
        }

        ObjectPool rightPool = objectPools[poolId];

        returnObj.SetActive(false);
        rightPool.pooledObjects.Add(returnObj);
    }
    public static void ReturnObjectToPool(GameObject returnObj){
        ObjectPool rightPool = GetPoolFromGameObject(returnObj);
        if(rightPool == null) {
            Debug.LogError("The Object you want to return isn't part of any pools. Please add it beforehand");
            return;
        }

        returnObj.SetActive(false);
        rightPool.pooledObjects.Add(returnObj);
    }

    #endregion


    static ObjectPool GetPoolFromGameObject(GameObject g){
        ObjectPool rightPool = null;
        string name = g.name;
        name = name.Substring(0, name.Length - 7);

        foreach (var item in objectPools)
        {
            if(item.startObject.name == name){
                rightPool = item;
            }
        }

        if(rightPool == null) Debug.LogWarning("Object not found in any Pools! Please add it to the list of objects");

        return rightPool;
    }
}

[Serializable]
public class ObjectPool{
    [HideInInspector] public int poolId;
    public GameObject startObject;
    [HideInInspector] public GameObject parentObject;
    [HideInInspector] public List<GameObject> pooledObjects = new List<GameObject>();
    public int startSize;
}