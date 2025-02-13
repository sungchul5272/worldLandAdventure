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


	[Networked] private NetworkDictionary<PlayerRef, string> _playerNames { get; set; }

	[SerializeField] Text _playerCount;
	[SerializeField] Text _sessionCode;
	[SerializeField] GameObject[] _playerList;
	[SerializeField] GameObject _runnerPrefab;

	UIManager _uiManager;
	NetworkRunner _runner;
	NetworkSceneManagerDefault _sceneManager;
	int _maxPlayers = 4;
	int _currentPlayer;
	bool _runnerInitialized = false;




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
		_uiManager = FindObjectOfType<UIManager>();
		_currentPlayer = 0;
	}



	private void Update()
	{
		if (!_runnerInitialized && _runner != null)
		{
			Debug.Log("NetworkRunner가 이제 초기화됨! OnPlayerJoined 호출 가능");
			_runnerInitialized = true;
		}
	}
	public void AddPlayer(PlayerRef player, string name)
	{
		if (!_playerNames.ContainsKey(player))
		{
			_playerNames.Add(player, name);
		}
		RefreshPlayerListUI();
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
		Debug.Log("방 생성 성공! (호스트)");
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

		var result = await _runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Client,  // 참가자는 클라이언트
			SessionName = sessionCode,
			SceneManager = _sceneManager
		});

		if (!result.Ok)
		{
			Debug.LogError($"방 참가 실패! 이유: {result.ShutdownReason}");
			return false;
		}

		_sessionCode.text = sessionCode;
		Debug.Log("방 참가 성공! (클라이언트)");
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
			Debug.Log("호스트가 방을 나감 세션 종료");
		}
		else // 클라이언트가 나가는 경우
		{
			await _runner.Shutdown();
			Debug.Log("클라이언트가 방을 나감");
		}
	}
	void RefreshPlayerListUI()
	{
		// 모든 UI 오브젝트 비활성화
		foreach (var obj in _playerList)
			obj.SetActive(false);

		_currentPlayer = _runner.ActivePlayers.Count();
		_playerCount.text = $"({_currentPlayer}/{_maxPlayers})";

		int index = 0;
		foreach (var player in _runner.ActivePlayers)
		{
			if (index < _playerList.Length)
			{
				_playerList[index].SetActive(true);
				Text nameText = _playerList[index].transform.GetChild(0).GetComponent<Text>();
				// PlayerManager에서 플레이어 이름을 가져옴
				//	string playerName = PlayerManager._instance.GetPlayerName(player) ?? $"Player {player.PlayerId}";
				//nameText.text = playerName;
			}
			index++;
		}
	}



	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void UpdateAllClientsUI(RpcInfo info = default)
	{
		RefreshPlayerListUI(); // 모든 클라이언트에서 UI 갱신
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
		Debug.Log($"플레이어 {player.PlayerId} 입장!");
		UpdateAllClientsUI();

	}


	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"플레이어 {player.PlayerId} 퇴장!");
		UpdateAllClientsUI();
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		Debug.Log($"서버 연결 종료: {reason}");
		_uiManager.ChangeUI("Lobby Screen");
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
