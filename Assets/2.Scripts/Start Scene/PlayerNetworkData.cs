using Fusion;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] public string PlayerName { get; set; }  // ��Ʈ��ũ���� �ڵ� ����ȭ�Ǵ� �÷��̾� �̸�
    [Networked] public int Gold { get; set; }           // �÷��̾� ������
    [Networked] public int Level { get; set; }          // �÷��̾� ����
    [Networked] public int Experience { get; set; }     // �÷��̾� ����ġ
    [Networked] public int Health { get; set; }         // �÷��̾� ü��

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
        if (Object.HasInputAuthority) // ���� �÷��̾ �ʱ�ȭ
        {
            LoadPlayerData(); // ������ �ε�
        }
    }

    public void LoadPlayerData()
    {
        // ���� ����� ������ �ҷ����� (���� ���� ��)
        PlayerName = PlayerPrefs.GetString("PlayerName", $"Player {Object.Id}");
        Gold = PlayerPrefs.GetInt("PlayerGold", 1000);
        Level = PlayerPrefs.GetInt("PlayerLevel", 1);
        Experience = PlayerPrefs.GetInt("PlayerExp", 0);
        Health = PlayerPrefs.GetInt("PlayerHealth", 100);
    }

    public void SavePlayerData()
    {
        // ���ÿ� ������ ���� (���� ���� ��)
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetInt("PlayerGold", Gold);
        PlayerPrefs.SetInt("PlayerLevel", Level);
        PlayerPrefs.SetInt("PlayerExp", Experience);
        PlayerPrefs.SetInt("PlayerHealth", Health);
        PlayerPrefs.Save();
    }
}
