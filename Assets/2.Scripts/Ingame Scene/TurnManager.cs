using Fusion;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager _instance;

    [Networked] private int currentTurnIndex { get; set; } = 0;
    [Networked] private int totalPlayers { get; set; } = 0;

    [Networked, Capacity(4)] private NetworkArray<PlayerRef> playerTurnOrder { get; }

    void Awake()
    {
        if (_instance == null) _instance = this;
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        totalPlayers = Runner.SessionInfo.PlayerCount;

        int index = 0;
        foreach (var player in Runner.ActivePlayers)
            playerTurnOrder.Set(index++, player);

        currentTurnIndex = 0;

        Debug.Log($"[TurnManager] 첫 번째 플레이어: {GetCurrentTurnPlayer()}");
        RPC_UpdateUI();
    }

    public void EndTurn()
    {
        if (!Object.HasStateAuthority) return;

        currentTurnIndex = (currentTurnIndex + 1) % totalPlayers;
        Debug.Log($"[TurnManager] 다음 턴: {GetCurrentTurnPlayer()}");

        RPC_UpdateUI();
    }

    public PlayerRef GetCurrentTurnPlayer()
    {
        return playerTurnOrder[currentTurnIndex];
    }

    public bool IsMyTurn()
    {
        return Runner.LocalPlayer == GetCurrentTurnPlayer();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateUI()
    {
        bool isMyTurn = IsMyTurn();
        Debug.Log($"[TurnManager] UI 업데이트 - 내 턴인가? {isMyTurn}");
        IngameManager._instance.SetDiceButtonState(isMyTurn);
    }
}
