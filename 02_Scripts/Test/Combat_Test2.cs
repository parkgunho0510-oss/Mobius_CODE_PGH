using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat_Test2 : MonoBehaviour
{
    [SerializeField] private PlayerStatus _playerStatus;
    
    
    [Header("Checker")]
    [SerializeField] private float _radius = 0.15f;
    [SerializeField] private LayerMask _layerMask;

    private Weapon _currentWeapon;

    private bool _isParryPhase;

    private Vector3[] _prevPoints = Array.Empty<Vector3>();
    private List<Transform> _castPoints = new();
    private HashSet<GameObject> _hitUnitSet = new();
    private RaycastHit[] _hits = new RaycastHit[32];

    public event Action<HashSet<GameObject>> UnitHitEvent;

    public void Anim_CanParryOpen() => _isParryPhase = true;
    public void Anim_CanParryClose() => _isParryPhase = false;



    public void Awake()
    {
        _playerStatus = GetComponent<PlayerStatus>();
    }



    void RebuildCastPoints()
    {
        _castPoints.Clear();

        if (_currentWeapon == null ||
            _currentWeapon.Tracer == null ||
            _currentWeapon.Tracer.Count == 0)
        {
            _prevPoints = Array.Empty<Vector3>();
            return;
        }

        foreach (var tracer in _currentWeapon.Tracer)
            _castPoints.Add(tracer);

        _prevPoints = new Vector3[_castPoints.Count];
        SyncPrevPoints();
    }

    void ApplyAttack()
    {
        if (_currentWeapon == null) return;

        //float damage = _currentWeapon.WeaponDB.ATK;  // ToDoList : Player Atk Stats Add
        float damage = _playerStatus.TotalATK;

        for (int i = 0; i < _castPoints.Count; i++)
        {
            Vector3 from = _prevPoints[i];
            Vector3 to = _castPoints[i].position;
            float dist = (to - from).magnitude;
            Vector3 dir = (to - from).normalized;

            Debug.DrawLine(from, to, Color.red, 0.1f);
            if (dist <= 0.001f) continue;

            int count = Physics.SphereCastNonAlloc(from, _radius, dir, _hits, dist, _layerMask);
            for (int j = 0; j < count; j++)
            {
                GameObject hitObj = _hits[j].collider.gameObject;
                if (!_hitUnitSet.Add(hitObj)) continue;

                // 검의 궤적(dir)과 검날이 향하는 방향(up)을 외적하여 절단 각도(Normal)를 미리 구합니다.
                Vector3 bladeDirection = _currentWeapon.transform.up;
                Vector3 sliceNormal = Vector3.Cross(dir, bladeDirection).normalized;

                // 체력이 있는가? 아니면 단순 환경 프랍인가?
                if (hitObj.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, gameObject, _isParryPhase, _hits[j].point, sliceNormal);
                }
                else if (hitObj.TryGetComponent<ISliceable>(out var sliceable))
                {
                    // 체력(IDamageable)이 없는 단순 나무, 상자 같은 환경 프랍일 경우에만 즉시 절단합니다.
                    sliceable.Slice(_hits[j].point, sliceNormal);
                }
            }
        }
    }

    void SyncPrevPoints()
    {
        for (int i = 0; i < _castPoints.Count; i++)
            _prevPoints[i] = _castPoints[i].position;
    }

    public void SetWeapon(Weapon newWeapon)
    {
        _currentWeapon = newWeapon;
        RebuildCastPoints();
    }

    public void StartCasting()
    {
        SyncPrevPoints();
        StartCoroutine(Co_Casting());
    }

    public void StopCasting()
    {
        UnitHitEvent?.Invoke(_hitUnitSet);
        StopAllCoroutines();
        _hitUnitSet.Clear();
        _isParryPhase = false;
    }

    public void Anim_Open()
    {
        StartCasting();
        CombatEventBus.RaiseAttackBegin();
    }
    public void Anim_Close()
    {
        StopCasting();
        CombatEventBus.RaiseAttackEnd();
    }
    IEnumerator Co_Casting()
    {
        while (true)
        {
            ApplyAttack();
            SyncPrevPoints();
            yield return null;
        }
    }
}