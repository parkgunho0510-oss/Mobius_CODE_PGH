using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, GameObject attacker = null, bool isCanParry = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default);
}