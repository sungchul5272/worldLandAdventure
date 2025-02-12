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
	static RoomManager _uniqeInstance;



	[SerializeField] Text _playerCount;
	[SerializeField] Text _sessionCode;
	[SerializeField] GameObject[] _playerList;
	[SerializeField] GameObject _runnerPrefab;
	[SerializeField]  NetworkObject _playerPrefab;

	UIManager _uiManager;

	NetworkRunner _runner;
	NetworkSceneManagerDefault _sceneManager;
	PlayerRef _hostPlayer;
	int _maxPlayers = 4;
	int _currentPlayer;




	public static RoomManager _instance
	{
		get { return _uniqeInstance; }
	}

	void Awake()
	{
		_uniqeInstance = this;
		DontDestroyOnLoad(gameObject);

	}


	void Start()
	{
		_uiManager = FindObjectOfType<UIManager>();
		_currentPlayer = 0;
	}
	public async Task<bool> OpenRoom(string sessionCode)
	{
		if (_runner == null)
		{
			_runner = Instantiate(_runnerPrefab).GetComponent<NetworkRunner>();
			_runner.ProvideInput = true;
		}

		_runner.AddCallbacks(this);

		if (_sceneManager == null) // SceneManager 중복 추가 방지
		{
			_sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
		}

		var result = await _runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Host,  // 방장(호스트)
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
	void RefreshPlayerListUI1()
	{
		// UI 초기화
		foreach (var obj in _playerList)
		{
			obj.SetActive(false);
		}

		// 현재 접속한 플레이어 수 갱신
		_currentPlayer = _runner.ActivePlayers.Count();
		_playerCount.text = $"({_currentPlayer}/{_maxPlayers})";

		// 현재 접속한 플레이어 수만큼 UI 활성화
		int index = 0;
		foreach (var player in _runner.ActivePlayers)
		{
			if (index < _playerList.Length)
			{
				_playerList[index].SetActive(true);
				Text name = _playerList[index].transform.GetChild(0).GetComponent<Text>();
				name.text = $"Player {player.PlayerId}";
			}
			index++;
		}
	}

	void RefreshPlayerListUI()
	{
		foreach (var obj in _playerList)
		{
			obj.SetActive(false);
		}

		_currentPlayer = _runner.ActivePlayers.Count();
		_playerCount.text = $"({_currentPlayer}/{_maxPlayers})";
		Debug.Log($"현재 참여자 수: {_currentPlayer}/{_maxPlayers}");

		int index = 0;
		foreach (var player in _runner.ActivePlayers)
		{
			if (index < _playerList.Length)
			{
				_playerList[index].SetActive(true);
				Text name = _playerList[index].transform.GetChild(0).GetComponent<Text>();

				// 네트워크 오브젝트에서 플레이어 이름 가져오기
				NetworkObject playerObject = _runner.GetPlayerObject(player);
				if (playerObject == null)
				{
					Debug.LogWarning($" 플레이어 {player.PlayerId}의 NetworkObject를 찾을 수 없습니다!");
					name.text = $"Player {player.PlayerId} (Unknown)";
					continue;
				}

				PlayerNetworkData playerData = playerObject.GetComponent<PlayerNetworkData>();
				if (playerData == null)
				{
					Debug.LogWarning($" 플레이어 {player.PlayerId}의 PlayerNetworkData를 찾을 수 없습니다!");
					name.text = $"Player {player.PlayerId} (No Data)";
					continue;
				}

				name.text = playerData.PlayerName; // 네트워크 데이터에서 동기화된 이름 표시
			}
			index++;
		}
	}


	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	void UpdateAllClientsUI(RpcInfo info = default)
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
;

		if (runner.IsServer) //  서버(호스트)에서만 플레이어를 생성
		{
			NetworkObject playerObject = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);

			if (playerObject == null)
			{
				Debug.LogError(" NetworkObject가 생성되지 않았습니다! _playerPrefab을 확인하세요.");
				return;
			}

			PlayerNetworkData networkData = playerObject.GetComponent<PlayerNetworkData>();

			if (networkData == null)
			{
				Debug.LogError(" PlayerNetworkData 컴포넌트가 없습니다! PlayerPrefab을 확인하세요.");
				return;
			}

			if (player == runner.LocalPlayer) //  본인 플레이어일 경우
			{
				networkData.LoadPlayerData(); //  로컬에서 데이터 불러오기
			}
		}

		UpdateAllClientsUI();
	}




	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"플레이어 {player.PlayerId} 퇴장!");

		NetworkObject playerObject = runner.GetPlayerObject(player);
		if (playerObject != null)
		{
			runner.Despawn(playerObject); 
			Destroy(playerObject.gameObject);
		}

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
