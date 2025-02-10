using UnityEngine;
using Fusion;
using System.Collections;

public class IngameManager : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;
    public RoomManager roomManagerPrefab;

    private NetworkRunner _networkRunner;
    private RoomManager _roomManager;

    void Start()
    {
        _networkRunner = FindObjectOfType<NetworkRunner>();
        if (_networkRunner == null)
        {
            _networkRunner = Instantiate(networkRunnerPrefab);
            DontDestroyOnLoad(_networkRunner);
        }
    }

    public void CreateRoom(string sessionCode)
    {
        StartCoroutine(CreateRoomCoroutine(sessionCode));
    }

    private IEnumerator CreateRoomCoroutine(string sessionCode)
    {
        if (_roomManager != null)
        {
            Debug.LogError("이미 방이 생성되었습니다.");
            yield break;
        }

        //  NetworkRunner 실행
        var resultTask = _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        yield return new WaitUntil(() => resultTask.IsCompleted); //  완료될 때까지 기다림

        if (!resultTask.Result.Ok)
        {
            Debug.LogError($"NetworkRunner 시작 실패: {resultTask.Result.ErrorMessage}");
            yield break;
        }

        //  RoomManager가 네트워크에서 올바르게 생성될 때까지 기다림
        _roomManager = _networkRunner.Spawn(roomManagerPrefab);
        yield return new WaitUntil(() => _roomManager != null && _roomManager.Object.IsValid); //  Spawn 완료될 때까지 대기

        if (_roomManager == null)
        {
            Debug.LogError("RoomManager를 Spawn하는 데 실패했습니다.");
            yield break;
        }

        DontDestroyOnLoad(_roomManager);
        _roomManager.SetNetworkRunner(_networkRunner);
        _roomManager.CreateRoom(sessionCode);
    }

    public void JoinRoom(string sessionCode)
    {
        StartCoroutine(JoinRoomCoroutine(sessionCode));
    }

    private IEnumerator JoinRoomCoroutine(string sessionCode)
    {
        if (_roomManager != null)
        {
            Debug.LogError("이미 방에 참가 중입니다.");
            yield break;
        }

        var resultTask = _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        yield return new WaitUntil(() => resultTask.IsCompleted);

        if (!resultTask.Result.Ok)
        {
            Debug.LogError($"NetworkRunner 시작 실패: {resultTask.Result.ErrorMessage}");
            yield break;
        }

        _roomManager = _networkRunner.Spawn(roomManagerPrefab);
        yield return new WaitUntil(() => _roomManager != null && _roomManager.Object.IsValid);

        if (_roomManager == null)
        {
            Debug.LogError("RoomManager를 Spawn하는 데 실패했습니다.");
            yield break;
        }

        DontDestroyOnLoad(_roomManager);
        _roomManager.SetNetworkRunner(_networkRunner);
        _roomManager.JoinRoom();
    }

    public void LeaveRoom()
    {
        if (_roomManager != null)
        {
            _roomManager.LeaveRoom();
        }
        else
        {
            Debug.LogWarning("RoomManager가 없습니다. 방을 떠날 수 없습니다.");
        }
    }
}
