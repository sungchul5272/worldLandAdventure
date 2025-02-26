using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngameManager : NetworkBehaviour
{
	static IngameManager _uniqueInstance;
	public static IngameManager _instance { get { return _uniqueInstance; } }

	[Networked] public int _currentTurnIndex { get; set; }
	[Networked] public int _diceResult { get; set; }

	[Networked, Capacity(8)] public NetworkLinkedList<PlayerRef> _playerList { get; }

	[SerializeField] Button _rollDiceButton;
	[SerializeField] Animator diceAnimator;
	[SerializeField] GameObject[] _characterObjects;
	[SerializeField] Transform[] _spawnPos;
	bool _isInitialized = false;
	float _animationDelayTime = 1f;

	void Awake()
	{
		_uniqueInstance = this;
		Debug.Log("IngameManager 실행됨");
	}

	public override void Spawned()
	{
		if (Object.HasStateAuthority)
		{
			List<PlayerRef> players = new List<PlayerRef>(Runner.ActivePlayers);
			SortPlayerRefs(players);
			_playerList.Clear();
			foreach (var player in players)
			{
				_playerList.Add(player);
			}

			if (players.Count > 0)
			{
				_currentTurnIndex = UnityEngine.Random.Range(0, players.Count);
			}
			else
			{
				_currentTurnIndex = 0;
			}
			_diceResult = 0;
			Debug.LogFormat("시작 턴 인덱스 : {0}", _currentTurnIndex);
		}
		_isInitialized = true;


	}


	public void OnRollDiceButtonClicked()
	{
		RPC_RequestDiceRoll();
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private void RPC_RequestDiceRoll(RpcInfo info = default)
	{
		if (!Object.HasStateAuthority)
		{
			return;
		}
		if (_currentTurnIndex < 0 || _currentTurnIndex >= _playerList.Count)
		{
			return;
		}


		PlayerRef expectedPlayer = GetPlayerAtIndex(_currentTurnIndex);

		int callerId = info.Source.PlayerId;
		if (callerId < 0)
		{
			callerId = Runner.LocalPlayer.PlayerId;
		}

		if (callerId != expectedPlayer.PlayerId)
		{
			return;
		}

		int roll = UnityEngine.Random.Range(1, 7);
		_diceResult = roll;
		Debug.LogFormat("주사위굴림 값 : {0} ", roll);

		RPC_PlayDiceAnimation(roll);

		ChangeTurn();
	}


	[Rpc(RpcSources.All, RpcTargets.All)]
	void RPC_PlayDiceAnimation(int roll, RpcInfo info = default)
	{
		if (diceAnimator != null)
		{
			diceAnimator.SetInteger("Value", roll);
			diceAnimator.SetBool("IsRolling", true);
			StartCoroutine(ResetIsRollingAfterDelay());
		}
	}

	IEnumerator ResetIsRollingAfterDelay()
	{
		yield return new WaitForSeconds(_animationDelayTime);
		if (diceAnimator != null)
			diceAnimator.SetBool("IsRolling", false);
	}

	void ChangeTurn()
	{
		int count = _playerList.Count;
		if (count == 0) return;

		_currentTurnIndex = (_currentTurnIndex + 1) % count;
		Debug.LogFormat("턴 종료 다음 플레이어 인덱스 : {0} ", _currentTurnIndex);
	}

	PlayerRef GetPlayerAtIndex(int index)
	{
		int i = 0;
		foreach (var player in _playerList)
		{
			if (i == index)
				return player;
			i++;
		}
		Debug.LogWarning($"GetPlayerAtIndex: Index {index} not found. PlayerList.Count = {_playerList.Count}");
		return default;
	}


	void SortPlayerRefs(List<PlayerRef> players)
	{
		int count = players.Count;
		for (int i = 0; i < count - 1; i++)
		{
			for (int j = 0; j < count - i - 1; j++)
			{
				if (players[j].PlayerId > players[j + 1].PlayerId)
				{
					PlayerRef temp = players[j];
					players[j] = players[j + 1];
					players[j + 1] = temp;
				}
			}
		}
	}




}
