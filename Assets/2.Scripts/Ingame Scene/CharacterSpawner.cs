using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    // 네트워크 프리팹 배열 (예: 4종류의 캐릭터)
    [SerializeField] private GameObject[] _characterPrefabs;
    // 미리 지정된 4개의 스폰 위치
    [SerializeField] private Transform[] _spawnPos;

    public override void Spawned()
    {
        // 서버(또는 StateAuthority)가 인게임 씬 진입 후 모든 플레이어를 스폰
        if (Object.HasStateAuthority)
        {
            // 약간의 지연 후 스폰을 진행 (모든 플레이어가 인게임 씬에 완전히 진입했다고 가정)
            Invoke(nameof(SpawnAllPlayers), 2f);
        }
    }

    private void SpawnAllPlayers()
    {
        List<PlayerRef> players = new List<PlayerRef>(Runner.ActivePlayers);
        // 정렬: 모든 클라이언트에서 동일한 순서를 보장하기 위해 버블 정렬 사용
        for (int i = 0; i < players.Count - 1; i++)
        {
            for (int j = 0; j < players.Count - i - 1; j++)
            {
                if (players[j].PlayerId > players[j + 1].PlayerId)
                {
                    PlayerRef temp = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = temp;
                }
            }
        }

        // 각 플레이어별로 캐릭터 스폰
        for (int i = 0; i < players.Count; i++)
        {
            // 플레이어마다 선택한 캐릭터 인덱스는 별도의 네트워크 변수나 로비 정보로 저장되어 있어야 함.
            // 여기서는 예시로 i % _characterPrefabs.Length 로 결정합니다.
            int prefabIndex = i % _characterPrefabs.Length;
            Vector3 spawnPosition = _spawnPos[i % _spawnPos.Length].position;
            Quaternion spawnRotation = Quaternion.identity;

            Runner.Spawn(_characterPrefabs[prefabIndex], spawnPosition, spawnRotation, players[i]);
            Debug.Log($"서버: 플레이어 {players[i].PlayerId} 스폰. 프리팹 {prefabIndex} 위치: {spawnPosition}");
        }
    }
}
