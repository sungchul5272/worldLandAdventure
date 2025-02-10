using System;
using UnityEngine;
using Fusion;
using System.Collections;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }
    public event Action OnRoomUpdated;

    private NetworkRunner _networkRunner;

    public string SessionCode { get; private set; }

    [Networked] public int PlayerCount { get; set; } = 0;
    [Networked] private NetworkArray<PlayerInfo> PlayerList => default;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForNetworkRunner());
    }

    private IEnumerator WaitForNetworkRunner()
    {
        while (_networkRunner == null)
        {
            _networkRunner = FindObjectOfType<NetworkRunner>();
            yield return null;
        }
    }

    public void SetNetworkRunner(NetworkRunner runner)
    {
        _networkRunner = runner;
    }

    public async void CreateRoom(string sessionCode)
    {
        if (!Object.IsValid)
        {
            Debug.LogError("RoomManager가 네트워크에서 Spawn되지 않았습니다.");
            return;
        }

        SessionCode = sessionCode;
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionCode,
            SceneManager = _networkRunner.GetComponent<NetworkSceneManagerDefault>()
        };

        var result = await _networkRunner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log($"방 생성 성공: {sessionCode}");
            AddPlayer();
        }
        else
        {
            Debug.LogError($"방 생성 실패: {result.ErrorMessage}");
        }

        OnRoomUpdated?.Invoke();
    }

    public async void JoinRoom()
    {
        if (!Object.IsValid)
        {
            Debug.LogError("RoomManager가 네트워크에서 Spawn되지 않았습니다.");
            return;
        }

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = SessionCode,
            SceneManager = _networkRunner.GetComponent<NetworkSceneManagerDefault>()
        };

        var result = await _networkRunner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log($"방 참가 성공: {SessionCode}");
            AddPlayer();
        }
        else
        {
            Debug.LogError($"방 참가 실패: {result.ErrorMessage}");
        }

        OnRoomUpdated?.Invoke();
    }

    private void AddPlayer()
    {
        if (!HasStateAuthority) return;

        string playerName = PlayerPrefs.GetString("PlayerName", $"Player {PlayerCount + 1}");
        for (int i = 0; i < PlayerList.Length; i++)
        {
            if (string.IsNullOrEmpty(PlayerList[i].playerName.ToString()))
            {
                PlayerList.Set(i, new PlayerInfo { playerName = playerName });
                break;
            }
        }

        PlayerCount++;
        OnRoomUpdated?.Invoke();
    }

    private void RemovePlayer()
    {
        if (!HasStateAuthority) return;

        string playerName = PlayerPrefs.GetString("PlayerName");
        for (int i = 0; i < PlayerList.Length; i++)
        {
            if (PlayerList[i].playerName.ToString() == playerName)
            {
                PlayerList.Set(i, new PlayerInfo { playerName = "" });
                break;
            }
        }

        PlayerCount--;
        OnRoomUpdated?.Invoke();
    }

    public string GetPlayerList()
    {
        string playerNames = "";
        for (int i = 0; i < PlayerList.Length; i++)
            if (!string.IsNullOrEmpty(PlayerList[i].playerName.ToString()))
                playerNames += PlayerList[i].playerName + "\n";

        return playerNames;
    }

    public override void FixedUpdateNetwork()
    {
        OnRoomUpdated?.Invoke();
    }

    public void LeaveRoom()
    {
        if (_networkRunner == null)
        {
            Debug.LogWarning("NetworkRunner가 존재하지 않아 방을 나갈 수 없습니다.");
            return;
        }

        RemovePlayer();

        if (HasStateAuthority) //  호스트가 나가면 방을 종료
        {
            Debug.Log("호스트가 방을 나가므로 방을 닫습니다.");
            _networkRunner.Shutdown();
        }
        else //  클라이언트가 나가면 방에서만 퇴장
        {
            Debug.Log("클라이언트가 방을 나갔습니다.");
            _networkRunner.Disconnect(_networkRunner.LocalPlayer);
        }

        OnRoomUpdated?.Invoke();
    }

}
