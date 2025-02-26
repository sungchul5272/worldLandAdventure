using UnityEngine;

public class PlayerDataTmp : MonoBehaviour
{
    private static PlayerData _instance;
    public static PlayerData Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬 어딘가에 빈 오브젝트로 붙이거나
                // Resources.Load 등으로 로드해서 생성할 수도 있음
                var go = new GameObject("PlayerData");
                _instance = go.AddComponent<PlayerData>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private string _playerName = "Guest";

    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    public string GetPlayerName()
    {
        return _playerName;
    }
}
