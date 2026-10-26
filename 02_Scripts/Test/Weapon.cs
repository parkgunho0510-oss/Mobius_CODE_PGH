using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    [SerializeField] string _weaponID;
    [SerializeField] List<Transform> _tracer;
    [SerializeField] EquipmentData _weaponDB;

    public List<Transform> Tracer => _tracer;
    public EquipmentData WeaponDB => _weaponDB;

    void Awake()
    {
        _weaponDB = DataManager.Instance.GetWeapon(_weaponID);
    }
}
