using UnityEngine;

public class PlayerHP : MonoBehaviour
{
   
    [SerializeField]private float _currentHP;
    public float CurrentHP
    {
        get { return _currentHP; }

    }
 [SerializeField] private float _maxHP = 100;
    public float MaxHP
    {
        get { return _maxHP; }
    }



    private void Awake()
    {
        _currentHP = _maxHP;
    }

    private void Start()
    {
        
        UIEvents.RaiseHPChanged(_currentHP, _maxHP);
    }

    // 체력 회복
    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentHP = _currentHP + amount;

        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }

        
        UIEvents.RaiseHPChanged(_currentHP, _maxHP);
    }

    // 데미지 처리
    public void TakeDamage(float amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentHP = _currentHP - amount;

        if (_currentHP < 0)
        {
            _currentHP = 0;
        }

        
        UIEvents.RaiseHPChanged(_currentHP, _maxHP);

        
        if (_currentHP == 0)
        {
          
            Debug.Log("플레이어 사망");
        }
    }
}