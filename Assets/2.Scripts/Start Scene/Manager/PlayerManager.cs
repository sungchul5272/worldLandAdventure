using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager _instance { get; set; }
    public string LocalPlayerName { get; set; } = "Guest";

    [Networked] public NetworkString<_16> playerName { get; set; }
    [Networked] public bool isReady { get; set; }


    //static List<PlayerManager> _allPlayersList = new List<PlayerManager>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this; //  미리 LocalInstance 설정
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void Spawned()
    {

        if (Object.HasInputAuthority)
        {
            _instance = this;
            Invoke(nameof(SetLocalPlayerName), 0.2f);
        }

        if (Object.HasStateAuthority)
        {
            RoomManager._instance.UpdateAllClientsUI();
        }

    }


    void SetLocalPlayerName()
    {
        string cachedName = PlayerData._instance.GetPlayerName();
        RequestSetPlayerName(cachedName);
    }


    public void RequestSetPlayerName(string name)
    {
        if (!Object.HasInputAuthority)
        {
            return;
        }

        RPC_SetPlayerName(name);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetPlayerName(string name)
    {
        playerName = name;
        RoomManager._instance.UpdateAllClientsUI();
    }


    public void ToggleReadyState()
    {
        if (!Object.HasInputAuthority)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            return;
        }

        bool newReadyState = !isReady;
        RPC_SetReadyState(newReadyState);
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_SetReadyState(bool readyState)
    {
        isReady = readyState;
        RoomManager._instance.UpdateAllClientsUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSceneChange()
    {
        if (Object.HasStateAuthority)
        {
            RPC_ExecuteSceneChange();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ExecuteSceneChange()
    {
        if (Runner.SceneManager != null)
        {
            SceneRef gameSceneRef = Runner.SceneManager.GetSceneRef("2.IngameScene");
            SceneRef startSceneRef = Runner.SceneManager.GetSceneRef("1.StartScene");
            Runner.SceneManager.LoadScene(gameSceneRef, new NetworkLoadSceneParameters());
            Runner.SceneManager.UnloadScene(startSceneRef);
        }
    }
}


