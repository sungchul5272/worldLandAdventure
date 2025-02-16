using UnityEngine;

public class PlayerData : MonoBehaviour
{
    static PlayerData _uniqueInstance;

    public static PlayerData _instance
    {
        get { return _uniqueInstance; }
    }

    public string _playerName { get;  set; } = "None";

    void Awake()
    {
        if (_uniqueInstance != null && _uniqueInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        _uniqueInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetPlayerName(string name)
    {
        _playerName = name;
        Debug.Log($"[PlayerData] 플레이어 이름 저장됨: {name}");
    }

    public string GetPlayerName()
    {
        return _playerName;
    }
}
