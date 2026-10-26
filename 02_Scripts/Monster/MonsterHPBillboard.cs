using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class MonsterHPBillboard : MonoBehaviour
{
    [SerializeField] Slider _hpProgressBar;
    [SerializeField] MonsterBase _monsterBase;
    [SerializeField] private Camera _cam;
    float _maxHp;
    private void Awake()
    {
        if(_cam == null) _cam = Camera.main;
        if(_hpProgressBar == null) _hpProgressBar = GetComponentInChildren<Slider>();
        if (_monsterBase == null) _monsterBase = transform.GetComponentInParent<MonsterBase>();


        if (_cam == null) Debug.Log("Camera is NULL");
        if (_hpProgressBar == null) Debug.Log("ProgressBar is NULL");
        if (_monsterBase == null) Debug.Log("MonsterBase is NULL");
    }
    void OnEnable()
    {
        _hpProgressBar.value = 1f;
        _monsterBase.OnHitEvent += HandleOnHit;
    }
    void OnDisable()
    {
        
        _monsterBase.OnHitEvent -= HandleOnHit;
    }
    void Start()
    {
        StartCoroutine(Co_SetMaxHp());
    }
    private void LateUpdate()
    {
        if (_cam == null) return;
        transform.forward = _cam.transform.forward;
    }

    IEnumerator Co_SetMaxHp()
    {
        yield return null;
        _maxHp = _monsterBase.MaxHp;
    }

    void HandleOnHit(float hp)
    {
        if (hp <= 0f) _hpProgressBar.value = 0f;
        else _hpProgressBar.value = hp / _maxHp;
    }
}