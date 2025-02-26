using UnityEngine;

public class PlayerDataTmp : MonoBehaviour
{
    static PlayerDataTmp _uniqueInstance;

    public static PlayerDataTmp _instance
    {
        get { return _uniqueInstance; }
    }

    public string _playerName { get; set; } = "None";

    void Awake()
    {
        if (_uniqueInstance != null && _uniqueInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        _uniqueInstance = this;
    }

    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    public string GetPlayerName()
    {
        return _playerName;
    }
}
