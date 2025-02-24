using UnityEngine;
using UnityEngine.UI;

public class IngameManager : MonoBehaviour
{
    public static IngameManager _instance;
    [SerializeField] private Button rollDiceButton;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    void Start()
    {
        rollDiceButton.onClick.AddListener(() =>
        {
            if (!TurnManager._instance.IsMyTurn()) return;

            Debug.Log("[IngameManager] 주사위 굴리기 요청");
            PlayerManager._instance.RequestRollDice();
            rollDiceButton.interactable = false;
        });

        rollDiceButton.interactable = false;
    }

    public void SetDiceButtonState(bool isActive)
    {
        Debug.Log($"[IngameManager] 주사위 버튼 활성화: {isActive}");
        rollDiceButton.interactable = isActive;
    }
}
