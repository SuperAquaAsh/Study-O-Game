using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkUI : NetworkBehaviour {
    public void StartHost(){
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient(){
        NetworkManager.Singleton.StartClient();
    }
    public void StartServer(){
        NetworkManager.Singleton.StartServer();
    }
}
