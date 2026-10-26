using UnityEngine;
using UnityEngine.UI;

public class MonsterHP : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private MonsterBase _monster;
    [SerializeField] private GameObject _fillObject;

    private void Start()
    {
        _hpSlider.maxValue = _monster.MaxHp;
        _hpSlider.value = _monster.CurrentHp;
    }

    private void Update()
    {
        _hpSlider.value = _monster.CurrentHp;

        if (_hpSlider.value <= 0f)
        {
            _fillObject.SetActive(false);
        }
    }
}