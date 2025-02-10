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
            Debug.LogError("�̹� ���� �����Ǿ����ϴ�.");
            yield break;
        }

        //  NetworkRunner ����
        var resultTask = _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        yield return new WaitUntil(() => resultTask.IsCompleted); //  �Ϸ�� ������ ��ٸ�

        if (!resultTask.Result.Ok)
        {
            Debug.LogError($"NetworkRunner ���� ����: {resultTask.Result.ErrorMessage}");
            yield break;
        }

        //  RoomManager�� ��Ʈ��ũ���� �ùٸ��� ������ ������ ��ٸ�
        _roomManager = _networkRunner.Spawn(roomManagerPrefab);
        yield return new WaitUntil(() => _roomManager != null && _roomManager.Object.IsValid); //  Spawn �Ϸ�� ������ ���

        if (_roomManager == null)
        {
            Debug.LogError("RoomManager�� Spawn�ϴ� �� �����߽��ϴ�.");
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
            Debug.LogError("�̹� �濡 ���� ���Դϴ�.");
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
            Debug.LogError($"NetworkRunner ���� ����: {resultTask.Result.ErrorMessage}");
            yield break;
        }

        _roomManager = _networkRunner.Spawn(roomManagerPrefab);
        yield return new WaitUntil(() => _roomManager != null && _roomManager.Object.IsValid);

        if (_roomManager == null)
        {
            Debug.LogError("RoomManager�� Spawn�ϴ� �� �����߽��ϴ�.");
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
            Debug.LogWarning("RoomManager�� �����ϴ�. ���� ���� �� �����ϴ�.");
        }
    }
}
