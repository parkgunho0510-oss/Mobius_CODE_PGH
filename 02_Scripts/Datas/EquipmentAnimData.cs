using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentAnimData", menuName = "Scriptable Objects/EquipmentAnimData")]
public class EquipmentAnimData : ScriptableObject
{
    [SerializeField] private RuntimeAnimatorController _weaponAnimatorController;
    [SerializeField] private int _maxComboCount = 3;

    public RuntimeAnimatorController WeaponAnimatorController { get { return _weaponAnimatorController; }}
    public int MaxComboCount { get { return _maxComboCount; }}

}