using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerEffects : NetworkBehaviour
{
    [SerializeField] EffectsGroup[] particleSystems;
    public void SpwanEffect(int id){
        Vector3 pos = transform.position;

        if(id == 2) pos -= Vector3.up * 0.1f;

        EffectsManager.SpwanObjectFromPool(id, pos, Quaternion.identity);
    }

    bool CheckEffectExists(int id){
        return particleSystems[id] != null;
    }

    #region Start Effect
    public void StartEffectNetworked(int id){
        if(!CheckEffectExists(id)){
            Debug.LogWarning("Effect id: " + id + " doesn't exist!");
            return;
        }

        StartEffectServerRpc(id);
        StartEffect(id);
    }

    [ServerRpc]
    void StartEffectServerRpc(int id, ServerRpcParams rpcParams = default){
        List<ulong> sendIds = NetworkManager.ConnectedClientsIds.ToList();
        sendIds.Remove(rpcParams.Receive.SenderClientId);

        ClientRpcParams clientRpc = new ClientRpcParams(){
            Send = new ClientRpcSendParams(){
                TargetClientIds = sendIds
            }
        };

        StartEffectClientRpc(id, clientRpc);

    }

    [ClientRpc]
    void StartEffectClientRpc(int id, ClientRpcParams rpcParams = default){
        StartEffect(id);
    }

    public void StartEffect(int id){
        particleSystems[id].StartEffect();
    }
    #endregion

    #region Stop Effect
    public void StopEffectNetworked(int id){
        if(!CheckEffectExists(id)){
            Debug.LogWarning("Effect id: " + id + " doesn't exist!");
            return;
        }

        StopEffectServerRpc(id);
        StopEffect(id);
    }

    [ServerRpc]
    void StopEffectServerRpc(int id, ServerRpcParams rpcParams = default){
        List<ulong> sendIds = NetworkManager.ConnectedClientsIds.ToList();
        sendIds.Remove(rpcParams.Receive.SenderClientId);

        ClientRpcParams clientRpc = new ClientRpcParams(){
            Send = new ClientRpcSendParams(){
                TargetClientIds = sendIds
            }
        };

        StopEffectClientRpc(id, clientRpc);

    }

    [ClientRpc]
    void StopEffectClientRpc(int id, ClientRpcParams rpcParams = default){
        bool destroyParticles = false;

        if(id == 1) destroyParticles = true;

        StopEffect(id, destroyParticles);
    }
    public void StopEffect(int id, bool destroyParticles = false){
        particleSystems[id].StopEffect(destroyParticles);
    }

    #endregion
}
