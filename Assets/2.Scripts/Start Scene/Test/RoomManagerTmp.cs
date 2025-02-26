using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class RoomManagerTmp : MonoBehaviour, INetworkRunnerCallbacks
{
    public static RoomManagerTmp _instance { get; private set; }

    [SerializeField] private NetworkRunner _runnerPrefab;
    [SerializeField] private NetworkObject _playerPrefab;
    private NetworkRunner _runner;
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



	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}


	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
	}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = Vector3.zero;  // 필요에 따라 스폰 위치를 설정하세요.
        Quaternion spawnRotation = Quaternion.identity;
        runner.Spawn(_playerPrefab, spawnPosition, spawnRotation, player);
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
