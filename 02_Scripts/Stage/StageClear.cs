using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageClear : MonoBehaviour
{

    [Header("Stage Settings")]
    [SerializeField] private bool _isBossStage;

    [SerializeField] GameObject _portalObject;
    [SerializeField] CinemachineCamera _clearCamera;

    [SerializeField]  private BossBase _boss;

    private void Start()
    {
        if (_isBossStage)
        {
            if (_boss != null)
            {
                StartCoroutine(Co_WaitForBossDeath());
            }
            else
            {
                Debug.LogWarning("BossBase를 가진 오브젝트가 없습니다.");
            }
        }
        else
        {
            MonsterEvents.MonsterDefeatedEvent += ShowClear;
        }
    }

    private void OnDestroy()
    {
        if (!_isBossStage)
        {
            MonsterEvents.MonsterDefeatedEvent -= ShowClear;
        }
    }
    private IEnumerator Co_WaitForBossDeath()
    {
        yield return new WaitUntil(() => _boss.IsDead);
        ShowClear();
    }
    private void ShowClear()
    {
        StartCoroutine(Co_ClearRoutine());
    }

    private IEnumerator Co_ClearRoutine()
    {
        _portalObject.SetActive(true);
        _clearCamera.Priority = 10;

        yield return new WaitForSeconds(2.0f); 

        _clearCamera.Priority = 0; 
    }
}