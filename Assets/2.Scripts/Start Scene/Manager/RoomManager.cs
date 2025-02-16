using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;



public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
    static RoomManager _uniqueInstance;

    public static RoomManager _instance
    {
        get { return _uniqueInstance; }
    }

    [SerializeField] NetworkRunner _runnerPrefab;
    [SerializeField] GameObject playerPrefab; // 플레이어 프리팹
    [SerializeField] GameObject[] _playerList; // UI 플레이어 리스트
    [SerializeField] Text _playerCount; // 플레이어 수 UI
    [SerializeField] Text _sessionCode; // 방 코드 표시
    [SerializeField] private Button startGameButton; //  게임 시작 버튼
    [SerializeField] private Sprite readyYellowSprite; //  흰별 아이콘 배열
    [SerializeField] private Sprite readyWhiteSprite;

    NetworkRunner _runner;
    NetworkSceneManagerDefault _sceneManager;

    int _maxPlayers = 4; // 최대 플레이어 수
    int _currentPlayer; // 현재 플레이어 수

    void Awake()
    {
        if (_uniqueInstance != null && _uniqueInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        _uniqueInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 방 생성 (호스트)
    /// </summary>
    public async Task<bool> OpenRoom(string sessionCode)
    {
        if (_runner == null)
        {
            _runner = Instantiate(_runnerPrefab).GetComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        _runner.AddCallbacks(this);

        if (_sceneManager == null)
            _sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionCode,
            SceneManager = _sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"방 생성 실패! 이유: {result.ShutdownReason}");
            return false;
        }

        _sessionCode.text = sessionCode;

        // 플레이어 이름을 `PlayerData`에서 가져와서 사용
        string playerName = PlayerData._instance.GetPlayerName();
        Debug.Log($"[RoomManager] 호스트 플레이어 이름 설정: {playerName}");

        //  SpawnNetworkPlayer()를 실행하지 않고, OnPlayerJoined()에서 실행되도록 변경
        return true;
    }


    /// <summary>
    /// 방 참가 (클라이언트)
    /// </summary>
    public async Task<bool> JoinRoom(string sessionCode)
    {
        if (_runner == null)
        {
            _runner = Instantiate(_runnerPrefab).GetComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        _runner.AddCallbacks(this);

        if (_sceneManager == null)
            _sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionCode,
            SceneManager = _sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"방 참가 실패! 이유: {result.ShutdownReason}");
            return false;
        }

        _sessionCode.text = sessionCode;

        Debug.Log("[RoomManager] 클라이언트 방 참가 성공!");

        //  플레이어 이름을 `PlayerData`에서 가져와서 사용
        string playerName = PlayerData._instance.GetPlayerName();
        Debug.Log($"[RoomManager] 클라이언트 플레이어 이름 설정: {playerName}");

        //  SpawnNetworkPlayer()를 실행하지 않고, OnPlayerJoined()에서 실행되도록 변경
        return true;
    }

    public async void LeaveRoom()
    {
        if (_runner == null)
            return;

        if (_runner.IsServer) // 호스트가 나가는 경우
        {
            await _runner.Shutdown();
            Destroy(_runner.gameObject);
            Debug.Log("호스트가 방을 나감. 세션 종료.");
        }
        else // 클라이언트가 나가는 경우
        {
            await _runner.Shutdown();
            Debug.Log("클라이언트가 방을 나감.");
        }

        ResetUI();
    }


    public void SpawnNetworkPlayer(NetworkRunner runner, PlayerRef playerRef, string playerName)
    {
        if (_runner == null || playerPrefab == null)
        {
            Debug.LogError("[RoomManager] 네트워크 러너 또는 플레이어 프리팹이 설정되지 않음.");
            return;
        }

        Debug.Log($"[RoomManager] 플레이어 생성 중... {playerRef}");

        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-3, 3), 1, UnityEngine.Random.Range(-3, 3));
        NetworkObject playerObject = _runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, playerRef);

        if (playerObject == null)
        {
            Debug.LogError("[RoomManager] 플레이어 네트워크 오브젝트 생성 실패!");
            return;
        }

        Debug.Log("[RoomManager] 네트워크 오브젝트가 생성됨. PlayerManager를 찾는 중...");

        PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
        if (playerManager != null)
        {
            Debug.Log($"[RoomManager] 플레이어 생성 완료: {playerName}");

            //  이제 여기서는 `RequestSetPlayerName()`을 실행하지 않음!
            // 플레이어가 `Spawned()`에서 InputAuthority를 받은 후 실행됨.
        }
        else
        {
            Debug.LogError("[RoomManager] PlayerManager를 찾을 수 없음! 프리팹 설정을 확인하세요.");
        }
    }



    /// <summary>
    /// 모든 클라이언트 UI 갱신 (네트워크 RPC)
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void UpdateAllClientsUI()
    {
        RefreshPlayerListUI();
    }

    /// <summary>
    /// 플레이어 리스트 UI 갱신
    /// </summary>
    void RefreshPlayerListUI()
    {
        foreach (var obj in _playerList) obj.SetActive(false);

        int index = 0;
        foreach (var player in _runner.ActivePlayers)
        {
            if (index >= _playerList.Length) break;

            _playerList[index].SetActive(true);
            Text nameText = _playerList[index].transform.GetChild(0).GetComponent<Text>();
            Image readyImage = _playerList[index].transform.GetChild(1).GetComponent<Image>();

            PlayerManager playerManager = FindObjectsOfType<PlayerManager>().FirstOrDefault(p => p.Object.InputAuthority == player);
            if (playerManager != null)
            {
                nameText.text = playerManager.playerName.ToString();
                readyImage.sprite = playerManager.isReady ? readyYellowSprite : readyWhiteSprite;
            }
            index++;
        }
    }



    /// <summary>
    /// UI 초기화 (방 떠날 때)
    /// </summary>
    private void ResetUI()
    {
        _sessionCode.text = "No Room";
        _playerCount.text = "(0/4)";

        foreach (var obj in _playerList)
            obj.SetActive(false);

        Debug.Log("UI 초기화 완료.");
    }
    void ForceUIUpdate()
    {
        Debug.Log("[RoomManager] 강제 UI 업데이트 실행.");
        UpdateAllClientsUI();
    }

    // ------------------CallBack 함수들----------------------------------------------------------------



    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[RoomManager] 플레이어 참가: {player}");

        if (runner.IsServer || runner.IsClient)
        {
            Debug.Log("[RoomManager] 네트워크 플레이어 스폰 실행");

            //  `PlayerData`에서 이름을 가져옴
            string playerName = PlayerData._instance.GetPlayerName();

            //  이제 플레이어가 입장하면 자동으로 생성
            SpawnNetworkPlayer(runner, player, playerName);
        }
        else
        {
            Debug.LogWarning("[RoomManager] 플레이어가 참가했지만, 서버나 클라이언트가 아님!");
        }
        UpdateAllClientsUI();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"플레이어 {player.PlayerId} 퇴장!");
        UpdateAllClientsUI();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ResetUI();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        UpdateAllClientsUI();

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

}
