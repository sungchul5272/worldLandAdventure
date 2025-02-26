using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RoomManagerTmp : MonoBehaviour, INetworkRunnerCallbacks
{
    public static RoomManagerTmp _instance { get; private set; }

    [SerializeField]  NetworkRunner _runnerPrefab;
    [SerializeField]  NetworkObject _playerPrefab;
    [SerializeField] Text _sessionCode;
    [SerializeField] GameObject[] _playerList;
    [SerializeField] Text _playerCount;
    [SerializeField] Sprite _readySprite;
    [SerializeField] Sprite _unreadySprite;

    NetworkRunner _runner;

    int _maxPlayers = 4;
    int _currentPlayer;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> OpenRoom(string sessionCode)
    {
        _runner = Instantiate(_runnerPrefab);
        _runner.name = "NetworkRunner";
        _runner.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        _runner.AddCallbacks(this);
        _sessionCode.text = sessionCode;
        var result = await _runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log("방 생성 성공");
            return true;
        }
        else
        {
            Debug.LogError("방 생성 실패: " + result.ShutdownReason);
            return false;
        }
    }

    public async Task<bool> JoinRoom(string sessionCode)
    {
        _runner = Instantiate(_runnerPrefab);
        _runner.name = "NetworkRunner";
        _runner.ProvideInput = true;



        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        _runner.AddCallbacks(this);
        _sessionCode.text = sessionCode;
        var result = await _runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log("방 참가 성공");
            return true;
        }
        else
        {
            Debug.LogError("방 참가 실패: " + result.ShutdownReason);
            return false;
        }
    }

    public void LeaveRoom()
    {
        if (_runner != null)
        {
            _runner.Shutdown();
            Debug.Log("방 나가기: 네트워크 런너 종료");
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


    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}


	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
        UpdateAllClientsUI();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = Vector3.zero;  // 필요에 따라 스폰 위치를 설정하세요.
        Quaternion spawnRotation = Quaternion.identity;
        runner.Spawn(_playerPrefab, spawnPosition, spawnRotation, player);
        UpdateAllClientsUI();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
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
