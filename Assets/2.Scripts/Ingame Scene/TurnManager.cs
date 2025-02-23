using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    static TurnManager _uniqueInstance;

    public static TurnManager _instance
    {
        get { return _uniqueInstance; }
    }

    [Networked] private int _currentTurnPlayerIndex { get; set; } = 0;
    [Networked] private int _totalPlayers { get; set; } = 0;

    [SerializeField] GameObject _turnTextUI; // 턴 알림 UI
    Text _turnText;
    public Button nextTurnButton;

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
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(EndTurn);
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            _totalPlayers = Runner.SessionInfo.PlayerCount;
            StartTurn();
        }
    }

    void StartTurn()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log($"[TurnManager] 플레이어 {_currentTurnPlayerIndex}의 턴 시작");
            RPC_UpdateTurn(_currentTurnPlayerIndex);
        }
    }

    public void EndTurn()
    {
        if (!Object.HasStateAuthority) return;

        _currentTurnPlayerIndex = (_currentTurnPlayerIndex + 1) % _totalPlayers;
        StartTurn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_UpdateTurn(int playerIndex)
    {
        Debug.Log($"[TurnManager] 현재 턴: 플레이어 {playerIndex}");
    }

    private IEnumerator ShowTurnUI(int playerIndex)
    {
        if (_turnTextUI != null && _turnText != null)
        {
            _turnTextUI.SetActive(true);
            _turnText.text = "Your Turn!!!!!";
            yield return new WaitForSeconds(2f);
            _turnTextUI.SetActive(false);
        }
    }
}