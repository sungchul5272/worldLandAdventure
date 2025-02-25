using UnityEngine;

public class TurnManager : MonoBehaviour
{
	static TurnManager _uniqueInstance;

	public static TurnManager _instance
	{
		get { return _uniqueInstance; }
	}

	public int _currentTurnIndex { get; private set; } = 0;
	public int _playerCount { get; private set; } = 1;
	void Awake()
	{
		_uniqueInstance = this;
	}
	// 초기 플레이어 수와 시작 턴을 설정하는 함수 (호스트가 네트워크로 초기화할 때 호출)
	public void InitializeTurn(int playerCount, int startingTurn)
	{
		_playerCount = playerCount;
		_currentTurnIndex = startingTurn;
	}

	// 턴을 순환하여 변경하는 기능
	public void ChangeTurn()
	{
		_currentTurnIndex = (_currentTurnIndex + 1) % _playerCount;
	}
}
