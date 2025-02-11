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
	//		Debug.Log("방이 존재하지 않습니다. 아무런 동작 없이 리턴됩니다.");
	//		return; // 방이 존재하지 않으면 바로 리턴
	//	}

	//	Debug.Log("방에 성공적으로 참여했습니다.");
	//	// 방에 참여한 후 추가적인 로직을 여기에 작성할 수 있습니다.
	//}

	//public async void TryJoinRoom(string sessionCode)
	//{
	//	await JoinRoom(sessionCode);

	//	// 방이 존재하지 않으면 여기서 아무런 동작 없이 리턴됩니다.
	//	// 방이 존재하면 여기서 추가적인 로직을 실행할 수 있습니다.
	//	Debug.Log("방 참여 시도 완료.");
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
			Debug.Log("방이 존재하지 않습니다. 아무런 동작 없이 리턴됩니다.");
			return; // 방이 존재하지 않으면 바로 리턴
		}

		Debug.Log("방에 성공적으로 참여했습니다.");
		// 방에 참여한 후 추가적인 로직을 여기에 작성할 수 있습니다.
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
