using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

public class CreateRoom : MonoBehaviour, INetworkRunnerCallbacks
{

	NetworkRunner _runner;

	public async void OpenRoom(string sessionCode)
	{
		GameMode gameMode = GameMode.Shared;
		_runner = gameObject.AddComponent<NetworkRunner>();
		_runner.ProvideInput = true; // player move

		var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
		var sceneInfo = new NetworkSceneInfo();

		if (scene.IsValid)
		{
			sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
		}

		await _runner.StartGame(new StartGameArgs()
		{
			GameMode = gameMode,
			SessionName = sessionCode,
			Scene = scene,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});
	}


	//public async Task JoinRoom(string sessionCode)
	//{
	//	if (_runner == null)
	//	{
	//		_runner = gameObject.AddComponent<NetworkRunner>();
	//		_runner.ProvideInput = true; // player move
	//	}

	//	var result = await _runner.StartGame(new StartGameArgs()
	//	{
	//		GameMode = GameMode.Shared,
	//		SessionName = sessionCode,
	//		SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
	//	});

	//	if (!result.Ok)
	//	{
	//		Debug.Log("���� �������� �ʽ��ϴ�. �ƹ��� ���� ���� ���ϵ˴ϴ�.");
	//		return; // ���� �������� ������ �ٷ� ����
	//	}

	//	Debug.Log("�濡 ���������� �����߽��ϴ�.");
	//	// �濡 ������ �� �߰����� ������ ���⿡ �ۼ��� �� �ֽ��ϴ�.
	//}

	//public async void TryJoinRoom(string sessionCode)
	//{
	//	await JoinRoom(sessionCode);

	//	// ���� �������� ������ ���⼭ �ƹ��� ���� ���� ���ϵ˴ϴ�.
	//	// ���� �����ϸ� ���⼭ �߰����� ������ ������ �� �ֽ��ϴ�.
	//	Debug.Log("�� ���� �õ� �Ϸ�.");
	//}


	public async void JoinRoom(string sessionCode)
	{
		if (_runner == null)
		{
			_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.ProvideInput = true; // player move
		}

		var result = await _runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Shared,
			SessionName = sessionCode,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});

		if (!result.Ok)
		{
			Debug.Log("���� �������� �ʽ��ϴ�. �ƹ��� ���� ���� ���ϵ˴ϴ�.");
			return; // ���� �������� ������ �ٷ� ����
		}

		Debug.Log("�濡 ���������� �����߽��ϴ�.");
		// �濡 ������ �� �߰����� ������ ���⿡ �ۼ��� �� �ֽ��ϴ�.
	}


	public void OnConnectedToServer(NetworkRunner runner)
	{
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
	}
}
