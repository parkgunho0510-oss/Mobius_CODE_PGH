using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private int _currentStanmina = 100;
    [SerializeField] private int _maxStamina = 100;

    [Header("Regen")]
    [SerializeField] private float _regenDelay = 1f;
    [SerializeField] private float _regenInterval = 0.5f;
    [SerializeField] private int _regenAmount = 10;

    [Header("Test Input")]
    [SerializeField] private int _spendAmount = 10;

    private float _lastSpendTime;
    private float _regenTimer;

    public int CurrentMP
    {
        get { return _currentStanmina; }
    }

    public int MaxMP
    {
        get { return _maxStamina; }
    }

    private void Start()
    {
        UIEvents.RaiseStaminaChanged(_currentStanmina, _maxStamina);
    }

    private void Update()
    {
        TestInput();
        RegenMP();
    }

    private void TestInput()
    {
        //test input
        if (Input.GetMouseButtonDown(0))
        {
            SpendMP(_spendAmount);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            SpendMP(_spendAmount);
        }
        if ( Input.GetKeyDown(KeyCode.Space))
        {
           SpendMP(_spendAmount);
        }

    }

    public bool SpendMP(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (_currentStanmina < amount)
        {
            Debug.Log("MP ºÎÁ·");
            return false;
        }

        _currentStanmina -= amount;
        _lastSpendTime = Time.time;
        _regenTimer = 0f;

        UIEvents.RaiseStaminaChanged(_currentStanmina, _maxStamina);
        return true;
    }

    private void RegenMP()
    {
        if (_currentStanmina >= _maxStamina)
        {
            return;
        }

        if (Time.time - _lastSpendTime < _regenDelay)
        {
            return;
        }

        _regenTimer += Time.deltaTime;

        if (_regenTimer >= _regenInterval)
        {
            _regenTimer = 0f;
            RecoverMP(_regenAmount);
        }
    }

    public void RecoverMP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentStanmina += amount;

        if (_currentStanmina > _maxStamina)
        {
            _currentStanmina = _maxStamina;
        }

        UIEvents.RaiseStaminaChanged(_currentStanmina, _maxStamina);
    }
}