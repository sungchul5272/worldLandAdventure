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
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject[] _playerList;
    [SerializeField] Text _playerCount;
    [SerializeField] Text _sessionCode;
    [SerializeField] Button startGameButton;
    [SerializeField] Sprite _readySprite;
    [SerializeField] Sprite _unreadySprite;


    public NetworkRunner _runner;
    NetworkSceneManagerDefault _sceneManager;


    int _maxPlayers = 4;
    int _currentPlayer;

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
    void Start()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
    }
    public void StartGame()
    {
        if (_runner != null && _runner.IsServer)
        {
            Debug.Log("게임 시작 - 호스트가 시작함");
            PlayerManager._instance.RPC_RequestSceneChange();
        }
    }

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


        string playerName = PlayerData._instance.GetPlayerName();

        return true;
    }

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

        Debug.Log("클라이언트 방 참가 성공!");

        string playerName = PlayerData._instance.GetPlayerName();
        Debug.Log($"클라이언트 플레이어 이름 설정: {playerName}");

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
        if (!runner.IsServer)
        {
            Debug.Log("클라이언트는 네트워크 오브젝트를 직접 생성할 수 없습니다.");
            return;
        }

        Debug.Log($"플레이어 생성 중... {playerRef}");

        NetworkObject networkPlayer = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerRef);
        if (networkPlayer != null)
        {
            PlayerManager playerManager = networkPlayer.GetComponent<PlayerManager>();
            playerManager.RequestSetPlayerName(playerName);
        }
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void UpdateAllClientsUI()
    {
        RefreshPlayerListUI();
    }


    void RefreshPlayerListUI()
    {
        foreach (var obj in _playerList) obj.SetActive(false);

        _currentPlayer = _runner.ActivePlayers.Count();
        _playerCount.text = $"({_currentPlayer}/{_maxPlayers})"; // 현재 플레이어 수 표시

        Dictionary<PlayerRef, PlayerManager> playerDict = new Dictionary<PlayerRef, PlayerManager>();
        foreach (var playerManager in FindObjectsOfType<PlayerManager>())
        {
            if (playerManager.Object != null && playerManager.Object.IsValid)
            {
                playerDict[playerManager.Object.InputAuthority] = playerManager;
            }
        }

        int index = 0;
        bool needRetry = false; 

        foreach (var player in _runner.ActivePlayers)
        {
            if (index >= _playerList.Length) break;
            int playerRef = player.PlayerId;
            Debug.Log(playerRef);
            _playerList[index].SetActive(true);
            Text nameText = _playerList[index].transform.GetChild(0).GetComponent<Text>();
            Image readyImage = _playerList[index].transform.GetChild(1).GetComponent<Image>();

            if (playerDict.TryGetValue(player, out PlayerManager playerManager))
            {
                if (!string.IsNullOrEmpty(playerManager.playerName.ToString()))
                {
                    nameText.text = playerManager.playerName.ToString();
                }
                else
                {
                    nameText.text = "Loading...";
                    needRetry = true;
                }

                bool isReadyState = playerManager.Object.HasStateAuthority || playerManager.isReady;
                readyImage.sprite = isReadyState ? _readySprite : _unreadySprite;
            }
            else
            {
                nameText.text = "Waiting...";
                readyImage.sprite = _unreadySprite;
                needRetry = true;
            }
            index++;
        }

        if (needRetry)
        {
            Invoke(nameof(RefreshPlayerListUI), 0.5f);
        }

        Debug.Log($"플레이어 리스트 UI 업데이트 완료! 현재 플레이어 수: {_currentPlayer}/{_maxPlayers}");
    }



    void ResetUI()
    {
        _sessionCode.text = "No Room";
        _playerCount.text = "(0/4)";

        foreach (var obj in _playerList)
        {
            obj.SetActive(false);
        }
            
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
        Debug.Log($"플레이어 참가!: {player}");

        if (runner.IsServer)
        {
            string playerName = PlayerData._instance.GetPlayerName();
            SpawnNetworkPlayer(runner, player, playerName);
            PlayerManager._instance.isReady = true;
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
