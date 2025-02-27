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

	[SerializeField] NetworkRunner _runnerPrefab;
	[SerializeField] NetworkObject _playerPrefab;
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

		_runner.AddCallbacks(this);
		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Host,
			SessionName = sessionCode,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		};

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

		_runner.AddCallbacks(this);

		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Client,
			SessionName = sessionCode,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		};


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
		Debug.Log("호출");
	}


	void RefreshPlayerListUI()
	{
		// 1) 기존 UI 초기화
		foreach (var obj in _playerList)
			obj.SetActive(false);

		// 2) 현재 활성 플레이어들을 List로 변환
		List<PlayerRef> players = new List<PlayerRef>(_runner.ActivePlayers);

		// 3) 버블 정렬로 PlayerId 기준 오름차순 정렬
		for (int i = 0; i < players.Count - 1; i++)
		{
			for (int j = 0; j < players.Count - 1 - i; j++)
			{
				if (players[j].PlayerId > players[j + 1].PlayerId)
				{
					var temp = players[j];
					players[j] = players[j + 1];
					players[j + 1] = temp;
				}
			}
		}

		// 플레이어 수 UI 표시
		_currentPlayer = players.Count;
		_playerCount.text = $"({_currentPlayer}/{_maxPlayers})";

		// 4) PlayerRef → PlayerManager 매핑
		Dictionary<PlayerRef, PlayerManagerTmp> playerDict = new Dictionary<PlayerRef, PlayerManagerTmp>();
		foreach (var pm in FindObjectsOfType<PlayerManagerTmp>())
		{
			if (pm.Object != null && pm.Object.IsValid)
			{
				playerDict[pm.Object.InputAuthority] = pm;
			}
		}

		// 5) 정렬된 플레이어 리스트 순서대로 UI 구성
		int index = 0;
		bool needRetry = false;

		foreach (var playerRef in players)
		{
			if (index >= _playerList.Length) break;

			// 해당 슬롯 활성화
			_playerList[index].SetActive(true);

			// 슬롯 내 Text, Image 참조
			Text nameText = _playerList[index].transform.GetChild(0).GetComponent<Text>();
			Image readyImage = _playerList[index].transform.GetChild(1).GetComponent<Image>();

			// PlayerManager가 있으면 이름/레디 상태 표시
			if (playerDict.TryGetValue(playerRef, out PlayerManagerTmp playerManager))
			{
				// 아직 이름이 동기화되지 않았다면 "Loading..."
				if (!string.IsNullOrEmpty(playerManager.playerName.ToString()))
				{
					nameText.text = playerManager.playerName.ToString();
				}
				else
				{
					nameText.text = "Loading...";
					needRetry = true;
				}


				readyImage.sprite = playerManager.isReady ? _readySprite : _unreadySprite;

			}
			else
			{
				// 아직 PlayerManager를 찾지 못했다면 대기
				nameText.text = "Waiting...";
				readyImage.sprite = _unreadySprite;
				needRetry = true;
			}
			index++;
		}

		// "Loading..." 상태인 경우 일정 시간 뒤 재시도
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
		var playerObjects = FindObjectsOfType<PlayerManagerTmp>();
		foreach (var pm in playerObjects)
		{
			if (pm.Object != null && pm.Object.IsValid && pm.Object.InputAuthority == player)
			{
				runner.Despawn(pm.Object);
			}
		}
		UpdateAllClientsUI();
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		if (runner.IsServer)
		{
			Vector3 spawnPosition = Vector3.zero;  // 필요에 따라 위치 설정
			Quaternion spawnRotation = Quaternion.identity;
			runner.Spawn(_playerPrefab, spawnPosition, spawnRotation, player);
		}
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
