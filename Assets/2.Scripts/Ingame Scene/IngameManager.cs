using Fusion;
using Fusion.Sockets;
using System;
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
    [Networked] public int LocalPlayerIndex { get; set; } // 각 클라이언트의 순번
    [Networked, Capacity(4)] public NetworkLinkedList<PlayerRef> Players { get; }

    [SerializeField] Button _rollDiceButton;
    [SerializeField] GameObject[] _characterObjects;

    int _localPlayerIndex;
    bool hasSpawned = false;

    void Awake()
    {
        _uniqueInstance = this;
        Debug.Log("실행됨");

        // NetworkRunner가 씬 전환 시 삭제되지 않도록 설정
        if (Runner != null)
        {
            DontDestroyOnLoad(Runner.gameObject);
        }
    }

    public override void Spawned()
    {
        hasSpawned = true;

        if (Object.HasStateAuthority) // 호스트만 실행
        {
            // 플레이어 리스트 초기화
            Players.Clear();
            foreach (var player in Runner.ActivePlayers)
            {
                Players.Add(player);
            }

            // 각 클라이언트에게 순번 할당
            for (int i = 0; i < Players.Count; i++)
            {
                RPC_SetPlayerIndex(Players[i], i);
            }

            // 초기 턴 설정
            _currentTurnIndex = UnityEngine.Random.Range(0, Players.Count);
            InitializeTurn(Players.Count, _currentTurnIndex);
        }
    }

    void Update()
    {
        if (!hasSpawned) return;

        // 현재 턴인 플레이어의 PlayerRef
        PlayerRef currentTurnPlayer = Players[_currentTurnIndex];

        // 로컬 플레이어가 현재 턴인지 확인
        _rollDiceButton.interactable = (Runner.LocalPlayer == currentTurnPlayer);
    }

    public void OnRollDiceButtonClicked()
    {
        RPC_RequestDiceRoll();
        ChangeTurn();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RequestDiceRoll()
    {

        DiceManager._instance.RollDice();

    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPlayerIndex(PlayerRef player, int index)
    {
        if (player == Runner.LocalPlayer)
        {
            LocalPlayerIndex = index; // 로컬 플레이어의 순번 저장
            Debug.Log($"I am player {LocalPlayerIndex + 1}");
        }
    }

    // 턴 초기화
    public void InitializeTurn(int playerCount, int startingTurn)
    {
        _playerCount = playerCount;
        _currentTurnIndex = startingTurn;
        Debug.Log($"Turn initialized. Current turn: Player {_currentTurnIndex + 1}");
    }

    // 턴 변경
    public void ChangeTurn()
    {
        _currentTurnIndex = (_currentTurnIndex + 1) % Players.Count;
        Debug.Log($"Turn changed to player {_currentTurnIndex + 1}");
    }
}