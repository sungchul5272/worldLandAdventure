using Fusion;
using UnityEngine;
using System.Collections;

public class DiceManager : NetworkBehaviour
{
    public static DiceManager _instance;

    [SerializeField] private Animator diceAnimator;
    private System.Random random = new System.Random();

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestRollDice()
    {
        if (!Object.HasStateAuthority) return;

        int diceValue = random.Next(1, 7);
        Debug.Log($"[DiceManager] 서버에서 주사위 값 생성: {diceValue}");

        RPC_SendDiceResult(diceValue);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SendDiceResult(int diceValue)
    {
        Debug.Log($"[DiceManager] 모든 클라이언트에서 주사위 결과 적용: {diceValue}");
        StartCoroutine(PlayDiceAnimation(diceValue));
    }

    private IEnumerator PlayDiceAnimation(int diceValue)
    {
        diceAnimator.SetBool("IsRolling", true);
        yield return new WaitForSeconds(1.5f);
        diceAnimator.SetBool("IsRolling", false);
        diceAnimator.SetInteger("Value", diceValue);

        TurnManager._instance.EndTurn();
    }
}
