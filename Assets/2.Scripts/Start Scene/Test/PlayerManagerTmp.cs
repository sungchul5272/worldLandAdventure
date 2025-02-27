using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManagerTmp : NetworkBehaviour
{
    // 로컬 플레이어의 PlayerManager를 전역적으로 참조할 수 있도록 합니다.
    static PlayerManagerTmp _uniqueInstance;

    public static PlayerManagerTmp _instance
    {
        get { return _uniqueInstance; }
    }

    // 네트워크 동기화된 플레이어 이름과 레디 상태
    [Networked] public NetworkString<_32> playerName { get; set; }
    [Networked] public bool isReady { get; set; }
    [Networked] public bool isHost { get; set; }

    // 로컬에서 임시로 보관할 이름 (초기값 "Guest")
    public string LocalPlayerName { get; set; } = "Guest";


    public override void Spawned()
    {

        if (Runner.IsServer)
        {
            // 호스트의 플레이어 객체: 서버에서 생성된 플레이어 객체 중 로컬 플레이어의 ID와 일치하는 경우
            // (호스트는 서버와 클라이언트 역할을 겸하므로, Runner.LocalPlayer가 호스트의 PlayerRef입니다.)
            if (Object.HasInputAuthority && Runner.LocalPlayer.PlayerId == Object.InputAuthority.PlayerId)
            {
                isHost = true;
                // 호스트는 자동으로 레디 상태 true
                RPC_SetReadyState(true);
            }
            else
            {
                isHost = false;
                RPC_SetReadyState(false);
            }
        }

        // 로컬 플레이어라면 일정 지연 후 로컬 저장된 이름을 서버에 전달
        if (Object.HasInputAuthority)
        {
            _uniqueInstance = this;
            Invoke(nameof(SetLocalPlayerName), 0.2f);
        }

        // (서버라면 UI 갱신 호출 - RoomManager가 존재할 경우)
        if (Object.HasStateAuthority && RoomManagerTmp._instance != null)
        {
            RoomManagerTmp._instance.UpdateAllClientsUI();
        }
    }

    void SetLocalPlayerName()
    {
        // PlayerData 싱글턴에서 저장된 이름을 가져와 서버에 요청합니다.
        string cachedName = PlayerDataTmp._instance.GetPlayerName();
        RequestSetPlayerName(cachedName);
    }

    public void RequestSetPlayerName(string name)
    {
        if (!Object.HasInputAuthority)
            return;

        RPC_SetPlayerName(name);
    }

    // 로컬 플레이어가 자신의 이름을 서버에 전달하는 RPC (InputAuthority → StateAuthority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetPlayerName(string name, RpcInfo info = default)
    {
        playerName = name;
        Debug.Log($"서버: 플레이어 {info.Source.PlayerId}의 이름이 {name}으로 설정되었습니다.");

        // 이름 변경 후 UI 갱신 (RoomManager가 네트워크 객체로 존재할 경우)
        if (RoomManagerTmp._instance != null)
        {
            RoomManagerTmp._instance.UpdateAllClientsUI();
        }
    }

    // 로컬 플레이어가 레디 토글을 요청합니다.
    public void ToggleReadyState()
    {
        if (!Object.HasInputAuthority)
            return;

        bool newReadyState = !isReady;
        RPC_SetReadyState(newReadyState);
    }

    // 모든 클라이언트에 레디 상태를 동기화하는 RPC (모든 소스, 모든 대상)
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SetReadyState(bool readyState, RpcInfo info = default)
    {
        // 만약 이 플레이어가 호스트라면 항상 true로 설정 (네트워크 변수 isHost가 복제되어 있음)
        if (isHost)
        {
            isReady = true;
        }
        else
        {
            isReady = readyState;
        }

        Debug.Log($"플레이어 {info.Source.PlayerId}의 레디 상태가 {isReady}로 설정되었습니다.");

        if (RoomManagerTmp._instance != null)
        {
            RoomManagerTmp._instance.UpdateAllClientsUI();
        }
    }


    // 로컬 플레이어가 씬 전환을 요청할 때 호출 (InputAuthority → StateAuthority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSceneChange(RpcInfo info = default)
    {
        if (Object.HasStateAuthority)
        {
            RPC_ExecuteSceneChange();
        }
    }

    // 서버(혹은 호스트)에서 모든 클라이언트에 씬 전환 명령을 내립니다.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ExecuteSceneChange(RpcInfo info = default)
    {
        if (Runner.SceneManager != null)
        {
            // 예시로 "2.IngameScene"으로 전환, "1.StartScene"을 언로드합니다.
            SceneRef gameSceneRef = Runner.SceneManager.GetSceneRef("2.IngameScene");
            SceneRef startSceneRef = Runner.SceneManager.GetSceneRef("1.StartScene");
            Runner.SceneManager.LoadScene(gameSceneRef, new NetworkLoadSceneParameters());
            Runner.SceneManager.UnloadScene(startSceneRef);
        }
    }
}
