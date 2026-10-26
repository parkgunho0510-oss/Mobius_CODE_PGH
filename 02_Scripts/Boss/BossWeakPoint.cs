using UnityEngine;

public class BossWeakPoint : MonoBehaviour, IDamageable
{
    private BossBase _boss;
    private SliceableObject _sliceable;
    private int _hitCount = 0;
    private const int MAX_HIT = 4;

    private void Awake()
    {
        _boss = GetComponentInParent<BossBase>();
        _sliceable = GetComponent<SliceableObject>();
    }

    public void TakeDamage(float damage, GameObject attacker = null, bool isParryable = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default)
    {
        // 보스가 존재하고, 오직 "그로기" 상태일 때만 타격 허용
        if (_boss != null && _boss.IsGroggy)
        {
            _hitCount++;

            if (_hitCount >= MAX_HIT)
            {
                BreakPart(hitPoint, hitNormal);
            }
        }
    }

    private void BreakPart(Vector3 point, Vector3 normal)
    {
        // 절단 실행 후, 성공적으로 절단된 경우에만 보스 방어력 감소
        if (_sliceable != null)
        {
            bool isSliced = _sliceable.Slice(point, normal);
            if (isSliced)
            {
                _boss.ReduceDefenseFromPartBreak();
            }
        }
    }
}