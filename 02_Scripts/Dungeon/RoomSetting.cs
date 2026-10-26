using UnityEngine;
using System.Collections.Generic;

public class RoomSetting : MonoBehaviour
{
    [Header("0:상, 1:하, 2:우, 3:좌")]
    [SerializeField] private GameObject[] walls; // 막힌 벽 오브젝트들
    [SerializeField] private GameObject[] doors; // 뚫린 문 오브젝트들

    [SerializeField] private List<Transform> monsterSpawnPoints;

    // 던전 제너레이터가 이 방을 스폰
    public void UpdateRoom(bool[] status, bool[] merged)
    {
        for (int i = 0; i < status.Length; i++)
        {
            // 방 병합 확인
            if (merged[i])
            {
                walls[i].SetActive(false); // 벽 제거
                doors[i].SetActive(false); // 문도 제거 (통로가 됨)
            }
            else
            {
                // status[i]가 true라는 것은 "이 방향으로 길이 뚫렸다"는 뜻입니다.
                doors[i].SetActive(status[i]);   // 길이 뚫리면 문 키기 
                walls[i].SetActive(!status[i]);  // 벽 끄기
            }
        }
    }

    public List<Transform> GetSpawnPoints()
    {
        return monsterSpawnPoints ?? new List<Transform>();
    }
}