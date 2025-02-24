using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager LocalInstance { get; set; }
    public string LocalPlayerName { get; set; } = "Guest";

    [Networked] public NetworkString<_16> playerName { get; set; }
    [Networked] public bool isReady { get; set; }

    //static List<PlayerManager> _allPlayersList = new List<PlayerManager>();

    void Awake()
    {
        if (LocalInstance == null)
        {
            LocalInstance = this; //  미리 LocalInstance 설정
            DontDestroyOnLoad(gameObject);
        }
        Debug.Log("[PlayerManager] Awake() 호출됨. 오브젝트가 생성됨.");
    }

    public override void Spawned()
    {
        Debug.Log("[PlayerManager] Spawned() 호출됨.");

        if (Object.HasInputAuthority)
        {
            LocalInstance = this;
            Debug.Log("[PlayerManager] 로컬 인스턴스 설정 완료.");
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

        Debug.Log("[PlayerManager] 플레이어 이름 설정 요청: " + name);
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


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReadyState(bool readyState)
    {
        isReady = readyState;
        RoomManager._instance.UpdateAllClientsUI();
    }
}


