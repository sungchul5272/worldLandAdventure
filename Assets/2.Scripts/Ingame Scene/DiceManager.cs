using UnityEngine;
using Fusion;

public class DiceManager : NetworkBehaviour
{
    static DiceManager _uniqueInstance;

    public static DiceManager _instance
    {
        get { return _uniqueInstance; }
    }

    private System.Random random = new System.Random();

    [Networked] private int lastDiceValue { get; set; }

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

    public void RollDice()
    {
        if (!Object.HasInputAuthority) return;
        RPC_RequestRollDice();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestRollDice()
    {
        int diceValue = random.Next(1, 7); // 1~6 랜덤 값 생성
        lastDiceValue = diceValue;
        Debug.Log($"[DiceManager] 주사위 굴림 결과: {diceValue}");
        RPC_SendDiceResult(diceValue);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SendDiceResult(int diceValue)
    {
        Debug.Log($"[DiceManager] 모든 플레이어에게 주사위 결과 전달: {diceValue}");
    }
}
