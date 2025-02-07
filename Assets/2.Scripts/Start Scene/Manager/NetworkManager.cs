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
			Debug.LogError("NetworkRunner�� �Ҵ���� �ʾҽ��ϴ�!");
			return;
		}
	}

	// �� ���� �Լ�
	public void CreateRoom()
	{
		string roomCode = roomCodeInput.text;

		if (string.IsNullOrEmpty(roomCode))
		{
			Debug.LogError("�� �ڵ带 �Է��ؾ� �մϴ�!");
			return;
		}

		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Shared,  // Shared ���� ���� (��Ƽ�÷��� ���)
		};



		// �� ����
		_runner.StartGame(startGameArgs);
		Debug.Log("���� �����Ǿ����ϴ�. �� �ڵ�: " + roomCode);
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