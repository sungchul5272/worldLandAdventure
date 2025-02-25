using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class IngameManager : NetworkBehaviour
{
	static IngameManager _uniqueInstance;

	public static IngameManager _instance
	{
		get { return _uniqueInstance; }
	}

	// 네트워크 동기화할 변수들
	[Networked] public int _diceResult { get; set; }
	[Networked] public int _currentTurnIndex { get; set; }
	[Networked] public int _playerCount { get; set; }


	[SerializeField] Button _rollDiceButton;
	[SerializeField] GameObject[] _characterPrefabs;
	[SerializeField] Transform[] _spawnPos;

	 Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
	int _localPlayerIndex;
	bool hasSpawned = false;



	void Awake()
	{
		_uniqueInstance = this;
		Debug.Log("실행됨");
	}

	public override void Spawned()
	{
		hasSpawned = true;

		if (Object.HasStateAuthority)
		{
			_playerCount = Runner.SessionInfo.PlayerCount;
			_currentTurnIndex = Random.Range(0, _playerCount);
			TurnManager._instance.InitializeTurn(_playerCount, _currentTurnIndex);
			int index = 0;

			foreach (PlayerRef player in Runner.ActivePlayers)
			{
				GameObject prefabToSpawn = _characterPrefabs[index % _characterPrefabs.Length];
				Transform spawnPoint = _spawnPos[index % _spawnPos.Length];
				NetworkObject spawnedCharacter = Runner.Spawn(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, player);
				if (spawnedCharacter != null)
				{
					DontDestroyOnLoad(spawnedCharacter.gameObject);
				}
				Debug.Log($"플레이어 {player.PlayerId}용 캐릭터 스폰됨: {prefabToSpawn.name}");
				index++;
			}
		}
	}

	void Update()
	{
		if (!hasSpawned)
			return;
		_rollDiceButton.interactable = (_localPlayerIndex == _currentTurnIndex);
	}

	public void OnRollDiceButtonClicked()
	{
		RPC_RequestDiceRoll();
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	public void RPC_RequestDiceRoll()
	{
		if (!Object.HasStateAuthority)
		{
			return;
		}

		int result = DiceManager._instance.RollDice();
		_diceResult = result;
		RPC_PlayDiceAnimation(result);
		TurnManager._instance.ChangeTurn();
		_currentTurnIndex = TurnManager._instance._currentTurnIndex;
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	public void RPC_PlayDiceAnimation(int result)
	{
		DiceManager._instance.PlayDiceAnimation(result);
	}

}