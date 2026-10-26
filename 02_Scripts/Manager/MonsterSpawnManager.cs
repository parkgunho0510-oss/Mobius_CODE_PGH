using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] PlayerController _player;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new();

    [Header("Current Stage")]
    [SerializeField] string _stageID;
    [SerializeField] private StageData _currentStageData;
    [SerializeField] private bool _DFS_Spawn = false;

    // 몬스터 프리팹을 Key로, 해당 몬스터의 풀을 Value로 가지는 딕셔너리
    // TODO : Key Change To Enum
    Dictionary<GameObject, ObjectPool<GameObject>> _poolTable = new();

    private int _monsterCount = 0;

    private void Awake()
    {
        MonsterEvents.MonsterReturnedEvent += HandleMonsterReturned;
    }

    private void OnDestroy()
    {
        MonsterEvents.MonsterReturnedEvent -= HandleMonsterReturned;
    }
    private void Start()
    {
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
        InitStage(_stageID);
    }
    private void InitStage(string stageID)
    {
        // DataManager에서 파싱 및 링크가 완료된 데이터 호출
        _currentStageData = DataManager.Instance.GetStage(stageID);

        if (_currentStageData == null)
        {
            Debug.LogError($"{stageID} 데이터를 찾을 수 없습니다!");
            return;
        }

        if (!_DFS_Spawn)
        {
            SpawnStageMonsters();
        }
    }
    private void InitPool()
    {
        foreach (var info in _currentStageData.MonsterList)
        {
            if (_poolTable.ContainsKey(info.MonsterPrefab)) continue;

            _poolTable[info.MonsterPrefab] = new ObjectPool<GameObject>(info.MaxSpawnCount, () =>
            {
                GameObject pref = Instantiate(info.MonsterPrefab);
                pref.transform.parent = transform; // 하이라키 정리를 위해 자식으로 설정
                pref.SetActive(false);

                if (pref.TryGetComponent(out MonsterBase monsterBase))
                {
                    monsterBase.PoolKey = info.MonsterPrefab;
                }

                return pref;
            });
        }
    }
    public void SetSpawnPoints(List<Transform> points)
    {
        _spawnPoints = points;
    }
    public void SpawnStageMonsters()
    {

        if (_currentStageData == null) return;

        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            Debug.LogWarning("스폰 포인트 설정 X");
            return;
        }

        InitPool();

        foreach (var info in _currentStageData.MonsterList)
        {
            int finalSpawnCount = Random.Range(info.MinSpawnCount, info.MaxSpawnCount + 1);

            for (int i = 0; i < finalSpawnCount; i++)
            {
                int randomIndex = Random.Range(0, _spawnPoints.Count);
                Vector3 spawnPos = _spawnPoints[randomIndex].position;
                Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

                GameObject monster = _poolTable[info.MonsterPrefab].Get();

                if (monster.TryGetComponent(out NavMeshAgent agent))
                {
                    agent.Warp(spawnPos + offset);
                }
                else
                {
                    monster.transform.position = spawnPos + offset;
                }

                monster.transform.rotation = Quaternion.identity;
                monster.SetActive(true);
                _monsterCount++;

                if (monster.TryGetComponent(out MonsterBase monsterBase))
                {
                    if (_player != null)
                    {
                        monsterBase.SetTarget(_player.gameObject);
                    }

                    monsterBase.Initialize();
                }
            }
        }
    }

    private void HandleMonsterReturned(GameObject poolKey, GameObject monsterObj)
    {
        if (poolKey == null)
        {
            Debug.Log("Pool Key Error!");
            return;
        }

        monsterObj.SetActive(false);
        // TODO: 몬스터 초기화

        _poolTable[poolKey].Set(monsterObj);

        _monsterCount--; // 죽어서 들어갈 때 -1 추가

        if (_monsterCount <= 0)
        {
            MonsterEvents.RaiseMonsterDefeatedEvent(); // 다 죽으면 이벤트 쏘기
        }
    }
}