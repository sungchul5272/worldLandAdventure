using Fusion;

using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
	NetworkRunner _runner;
	public InputField roomCodeInput;

	void Start()
	{
		_runner = GetComponent<NetworkRunner>();

		if (_runner == null)
		{
			Debug.LogError("NetworkRunner가 할당되지 않았습니다!");
			return;
		}
	}

	// 방 생성 함수
	public void CreateRoom()
	{
		string roomCode = roomCodeInput.text;

		if (string.IsNullOrEmpty(roomCode))
		{
			Debug.LogError("방 코드를 입력해야 합니다!");
			return;
		}

		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Shared,  // Shared 모드로 설정 (멀티플레이 방식)
		};



		// 방 생성
		_runner.StartGame(startGameArgs);
		Debug.Log("방이 생성되었습니다. 방 코드: " + roomCode);
	}

	public async Task JoinLobby(NetworkRunner runner)
	{

		var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

		if (result.Ok)
		{
			// all good
		}
		else
		{
			Debug.LogError($"Failed to Start: {result.ShutdownReason}");
		}
	}
}