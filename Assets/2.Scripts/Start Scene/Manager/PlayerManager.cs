using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager _instance;

    //  네트워크 변수는 Spawned 이후에만 접근 가능하므로, 일반 변수로 임시 저장
    private string _localPlayerName;

    [Networked] public string PlayerName { get; private set; } // Spawned 이후에만 접근 가능

    private void Awake()
    {
        _instance = this;
    }

    //  UIManager에서 이름을 저장할 때 Networked 변수를 사용하지 않고 일반 변수 사용
    public void SetLocalPlayerName(string name)
    {
        _localPlayerName = name;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority) // 본인만 실행 가능
        {
            SendNameToServer();
        }
    }

    public void SendNameToServer()
    {
        if (!Object || !Object.IsValid) return;

        //  Networked 변수를 직접 설정하지 않고 RPC로 서버에 전달
        if (!string.IsNullOrEmpty(_localPlayerName))
        {
            RPC_SetPlayerName(_localPlayerName);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerName(string name, RpcInfo info = default)
    {
        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            RoomManager._instance.AddPlayer(info.Source, name);
        }
    }
}
