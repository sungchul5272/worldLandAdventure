using Fusion;
using UnityEngine;

public class PlayerManagerTmp : NetworkBehaviour
{
    // 네트워크 동기화된 플레이어 이름 변수 (최대 32글자)
    [Networked] public NetworkString<_32> playerName { get; set; }

    public override void Spawned()
    {
        // 로컬 플레이어라면 내 이름을 서버에 전달합니다.
        if (Object.HasInputAuthority)
        {
            string localName = PlayerDataTmp.Instance.GetPlayerName();
            RPC_SetPlayerName(localName);
        }
    }

    // 로컬 플레이어의 이름을 서버로 전달하는 RPC (InputAuthority → StateAuthority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name, RpcInfo info = default)
    {
        playerName = name;
        Debug.Log($"서버: 플레이어 {info.Source.PlayerId}의 이름이 {name}으로 설정되었습니다.");
    }
}
