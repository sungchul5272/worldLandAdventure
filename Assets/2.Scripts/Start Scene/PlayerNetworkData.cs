using Fusion;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] public string PlayerName { get; set; }  // 네트워크에서 자동 동기화되는 플레이어 이름
    [Networked] public int Gold { get; set; }           // 플레이어 소지금
    [Networked] public int Level { get; set; }          // 플레이어 레벨
    [Networked] public int Experience { get; set; }     // 플레이어 경험치
    [Networked] public int Health { get; set; }         // 플레이어 체력

    static PlayerNetworkData _uniqeInstance;
    public static PlayerNetworkData _instance
    {
        get { return _uniqeInstance; }
    }

    void Awake()
    {
        _uniqeInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority) // 본인 플레이어만 초기화
        {
            LoadPlayerData(); // 데이터 로드
        }
    }

    public void LoadPlayerData()
    {
        // 에서 저장된 데이터 불러오기 (게임 시작 시)
        PlayerName = PlayerPrefs.GetString("PlayerName", $"Player {Object.Id}");
        Gold = PlayerPrefs.GetInt("PlayerGold", 1000);
        Level = PlayerPrefs.GetInt("PlayerLevel", 1);
        Experience = PlayerPrefs.GetInt("PlayerExp", 0);
        Health = PlayerPrefs.GetInt("PlayerHealth", 100);
    }

    public void SavePlayerData()
    {
        // 로컬에 데이터 저장 (게임 종료 시)
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetInt("PlayerGold", Gold);
        PlayerPrefs.SetInt("PlayerLevel", Level);
        PlayerPrefs.SetInt("PlayerExp", Experience);
        PlayerPrefs.SetInt("PlayerHealth", Health);
        PlayerPrefs.Save();
    }
}
