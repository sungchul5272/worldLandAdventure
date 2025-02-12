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

		if (_sceneManager == null) // SceneManager �ߺ� �߰� ����
		{
			_sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
		}

		var result = await _runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Host,  // ����(ȣ��Ʈ)
			SessionName = sessionCode,
			SceneManager = _sceneManager
		});

		if (!result.Ok)
		{
			Debug.LogError($"�� ���� ����! ����: {result.ShutdownReason}");
			return false;
		}

		_sessionCode.text = sessionCode;
		Debug.Log("�� ���� ����! (ȣ��Ʈ)");
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
			GameMode = GameMode.Client,  // �����ڴ� Ŭ���̾�Ʈ
			SessionName = sessionCode,
			SceneManager = _sceneManager
		});

		if (!result.Ok)
		{
			Debug.LogError($"�� ���� ����! ����: {result.ShutdownReason}");
			return false;
		}

		_sessionCode.text = sessionCode;
		Debug.Log("�� ���� ����! (Ŭ���̾�Ʈ)");
		return true;
	}


	public async void LeaveRoom()
	{
		if (_runner == null)
			return;

		if (_runner.IsServer) // ȣ��Ʈ�� ������ ���
		{
			await _runner.Shutdown();
			Destroy(_runner.gameObject);
			Debug.Log("ȣ��Ʈ�� ���� ���� ���� ����");
		}
		else // Ŭ���̾�Ʈ�� ������ ���
		{
			await _runner.Shutdown();
			Debug.Log("Ŭ���̾�Ʈ�� ���� ����");
		}
	}
	void RefreshPlayerListUI1()
	{
		// UI �ʱ�ȭ
		foreach (var obj in _playerList)
		{
			obj.SetActive(false);
		}

		// ���� ������ �÷��̾� �� ����
		_currentPlayer = _runner.ActivePlayers.Count();
		_playerCount.text = $"({_currentPlayer}/{_maxPlayers})";

		// ���� ������ �÷��̾� ����ŭ UI Ȱ��ȭ
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
		Debug.Log($"���� ������ ��: {_currentPlayer}/{_maxPlayers}");

		int index = 0;
		foreach (var player in _runner.ActivePlayers)
		{
			if (index < _playerList.Length)
			{
				_playerList[index].SetActive(true);
				Text name = _playerList[index].transform.GetChild(0).GetComponent<Text>();

				// ��Ʈ��ũ ������Ʈ���� �÷��̾� �̸� ��������
				NetworkObject playerObject = _runner.GetPlayerObject(player);
				if (playerObject == null)
				{
					Debug.LogWarning($" �÷��̾� {player.PlayerId}�� NetworkObject�� ã�� �� �����ϴ�!");
					name.text = $"Player {player.PlayerId} (Unknown)";
					continue;
				}

				PlayerNetworkData playerData = playerObject.GetComponent<PlayerNetworkData>();
				if (playerData == null)
				{
					Debug.LogWarning($" �÷��̾� {player.PlayerId}�� PlayerNetworkData�� ã�� �� �����ϴ�!");
					name.text = $"Player {player.PlayerId} (No Data)";
					continue;
				}

				name.text = playerData.PlayerName; // ��Ʈ��ũ �����Ϳ��� ����ȭ�� �̸� ǥ��
			}
			index++;
		}
	}


	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	void UpdateAllClientsUI(RpcInfo info = default)
	{
		RefreshPlayerListUI(); // ��� Ŭ���̾�Ʈ���� UI ����
	}


	// ------------------CallBack �Լ���----------------------------------------------------------------



	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}


	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"�÷��̾� {player.PlayerId} ����!");
;

		if (runner.IsServer) //  ����(ȣ��Ʈ)������ �÷��̾ ����
		{
			NetworkObject playerObject = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);

			if (playerObject == null)
			{
				Debug.LogError(" NetworkObject�� �������� �ʾҽ��ϴ�! _playerPrefab�� Ȯ���ϼ���.");
				return;
			}

			PlayerNetworkData networkData = playerObject.GetComponent<PlayerNetworkData>();

			if (networkData == null)
			{
				Debug.LogError(" PlayerNetworkData ������Ʈ�� �����ϴ�! PlayerPrefab�� Ȯ���ϼ���.");
				return;
			}

			if (player == runner.LocalPlayer) //  ���� �÷��̾��� ���
			{
				networkData.LoadPlayerData(); //  ���ÿ��� ������ �ҷ�����
			}
		}

		UpdateAllClientsUI();
	}




	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"�÷��̾� {player.PlayerId} ����!");

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
		Debug.Log($"���� ���� ����: {reason}");
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
