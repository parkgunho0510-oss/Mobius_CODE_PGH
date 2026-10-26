using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HitboxGroup
{
    public string GroupName;
    public BoxCollider[] Hitboxes;
    public ShakeData OnHitShake;
}

public class MonsterCombat : MonoBehaviour
{
    [Header("[Ref]")]
    [SerializeField] private MonsterDB _monsterDB;

    [Header("[Hitbox Settings]")]
    public HitboxGroup[] HitboxGroups;
    [SerializeField] private LayerMask _layerMask;

    private BoxCollider[] _activeHitboxes;
    private Vector3[] _prevPoints;
    private HashSet<GameObject> _hitUnitSet = new HashSet<GameObject>();
    private RaycastHit[] _hits = new RaycastHit[32];
    private HitboxGroup _currentActiveGroup;

    public event Action OnAttackFinished;
    private bool _isCasting = false;
    [Header("Parry Settings")]


    private bool _isParryPhase = false;

    void Start()
    {
        // MonsterBase(일반몹) 확인
        if (TryGetComponent<MonsterBase>(out var mob))
        {
            _monsterDB = mob.Data;
        }
        // BossBase(보스) 확인
        else if (TryGetComponent<BossBase>(out var boss))
        {
           
            _monsterDB = boss.MonsterDB;
        }

        //_monsterDB = GetComponent<MonsterBase>().Data;
    }

    public void Anim_CanParryOpen() => _isParryPhase = true;
    public void Anim_CanParryClose() => _isParryPhase = false;
    public void SetActiveHitboxGroup(string groupName)
    {
        foreach (var group in HitboxGroups)
        {
            if (group.GroupName == groupName)
            {
                _activeHitboxes = group.Hitboxes;
                _currentActiveGroup = group;
                return;
            }
        }
        Debug.LogWarning($"[{name}] Hitbox Group '{groupName}'을 찾을 수 없습니다!");
    }

    private void ApplyAttack()
    {
        if (_activeHitboxes == null || _activeHitboxes.Length == 0) return;

        float damage = (_monsterDB != null) ? _monsterDB.Damage : 0f;

        for (int i = 0; i < _activeHitboxes.Length; i++)
        {
            var box = _activeHitboxes[i];
            if (box == null) continue;

            // 큐브(BoxCollider)의 실제 월드 중심점과 크기를 정확히 계산
            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;

            Vector3 prevPos = _prevPoints[i];
            Vector3 diff = center - prevPos;
            float dist = diff.magnitude;

            // 제자리 공격이면 큐브의 정면으로 살짝 밀어 판정
            Vector3 dir = (dist > 0.001f) ? diff.normalized : box.transform.forward;

            // 큐브의 크기, 각도, 위치를 동일하게 복사해서 충돌 체크
            int count = Physics.BoxCastNonAlloc(
                prevPos,
                halfExtents,
                dir,
                _hits,
                box.transform.rotation,
                dist,
                _layerMask
            );

            for (int j = 0; j < count; j++)
            {
                GameObject hitObj = _hits[j].collider.gameObject;
                if (_hitUnitSet.Add(hitObj))
                {
                    if (hitObj.TryGetComponent<IDamageable>(out var target))
                    {
                        if (hitObj == gameObject) continue; // 자기 자신 타격 방지
                        target.TakeDamage(damage, gameObject, _isParryPhase, shakeData: _currentActiveGroup.OnHitShake);
                    }
                }
            }

            // 위치 업데이트
            _prevPoints[i] = center;
        }
    }

    #region Animation Events
    public void Anim_Open()
    {
        if (_activeHitboxes == null) return;

        _hitUnitSet.Clear();
        _prevPoints = new Vector3[_activeHitboxes.Length];

        // 캐스팅 시작 시 큐브의 초기 중심점 저장
        for (int i = 0; i < _activeHitboxes.Length; i++)
        {
            if (_activeHitboxes[i] != null)
                _prevPoints[i] = _activeHitboxes[i].transform.TransformPoint(_activeHitboxes[i].center);
        }

        _isCasting = true;
        StartCoroutine(Co_Casting());
    }

    public void Anim_Close()
    {
        ForceStopAttack();
        OnAttackFinished?.Invoke();
    }
    public void ForceStopAttack()
    {
        _isCasting = false;
        _isParryPhase = false;

        // 잔여 히트박스 정보 완전 제거 (허공 피격 방지)
        _activeHitboxes = null;
        _hitUnitSet.Clear();

        StopAllCoroutines();
    }
    public void StopCasting()
    {
        _isCasting = false;
        _isParryPhase = false;
        StopAllCoroutines();
    }

    private IEnumerator Co_Casting()
    {
        while (_isCasting)
        {
            ApplyAttack();
            yield return null;
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (_activeHitboxes == null || !_isCasting) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        foreach (var box in _activeHitboxes)
        {
            if (box == null) continue;
            Gizmos.matrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale);
            Gizmos.DrawCube(box.center, box.size);
        }
    }
    #endregion
}