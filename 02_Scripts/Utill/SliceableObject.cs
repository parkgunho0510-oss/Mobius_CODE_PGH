using UnityEngine;
using EzySlice;

public interface ISliceable
{
    bool Slice(Vector3 point, Vector3 normal);
}

public class SliceableObject : MonoBehaviour, ISliceable
{
    [SerializeField] private Material _crossSectionMaterial;
    [SerializeField] private float _separationForce = 3f;
    [SerializeField] private float _torqueForce = 5f;
    [SerializeField] private float _destroyDelay = 3f;
    [SerializeField] private LayerMask _ignoreLayers;
    private bool _isSliced = false;
    public bool Slice(Vector3 point, Vector3 normal)
    {
        if (_isSliced) return false;

        SlicedHull hull = gameObject.Slice(point, normal);

        if (hull != null)
        {
            _isSliced = true; // 중복 슬라이스 방지

            GameObject upper = hull.CreateUpperHull(gameObject, _crossSectionMaterial);
            GameObject lower = hull.CreateLowerHull(gameObject, _crossSectionMaterial);

            SetupSlicedComponent(upper, normal);
            SetupSlicedComponent(lower, -normal);

            Destroy(upper, _destroyDelay);
            Destroy(lower, _destroyDelay);
            Destroy(gameObject);

            return true;
        }
        return false;
    }

    private void SetupSlicedComponent(GameObject obj, Vector3 forceDirection)
    {
        Vector3 originalWorldPos = transform.position;

        //부모를 해제하고, 레이어를 설정합니다.
        obj.transform.SetParent(null, true);

        obj.transform.position = originalWorldPos + (Vector3.up * 1.2f);

        // 물리 컴포넌트 설정
        MeshCollider col = obj.AddComponent<MeshCollider>();
        col.convex = true;
        col.excludeLayers = _ignoreLayers;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Physics.SyncTransforms();

        // 힘 전달 (절단면 방향 + 월드 위쪽 힘을 섞어 자연스럽게 연출)
        Vector3 jumpDir = (forceDirection + Vector3.up * 1.5f).normalized;
        rb.AddForce(jumpDir * _separationForce, ForceMode.Impulse);
        rb.AddTorque(UnityEngine.Random.insideUnitSphere * _torqueForce, ForceMode.Impulse);
    }
}