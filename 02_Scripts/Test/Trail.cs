using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Trail : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private int corner = 1;
    [SerializeField] private bool loop;
    [SerializeField] private Transform[] pointTransforms = Array.Empty<Transform>();

    private GameObject trailGo;
    private Mesh mesh;
    private Vector3[] vertices, points = Array.Empty<Vector3>();
    private Transform cacheTM;
    private Coroutine updateCoroutine;
    private readonly WaitForFixedUpdate waitForFixedUpdate = new();

    private int lastSegmentCount = -1, lastCorner = -1, pointCount = -1;
    private float toCornerT;

    public Transform[] PointTransforms { get => pointTransforms; set => pointTransforms = value; }
    public bool Loop => loop && pointTransforms.Length >= 3;

    private void Start()
    {
        cacheTM = transform;
        trailGo = new GameObject($"{name}_TrailEffect", typeof(MeshFilter), typeof(MeshRenderer)) { layer = gameObject.layer };
        DontDestroyOnLoad(trailGo);
        trailGo.SetActive(false);

        mesh = new Mesh { name = "Trail Effect" };
        mesh.MarkDynamic();

        trailGo.GetComponent<MeshFilter>().sharedMesh = mesh;
        var mr = trailGo.GetComponent<MeshRenderer>();
        mr.material = material;
        mr.shadowCastingMode = ShadowCastingMode.Off;

        SyncPointsFromTransforms();
        Initialize((int)(duration / Time.fixedDeltaTime));
    }

    private void OnDestroy()
    {
        if (mesh) Destroy(mesh);
        if (trailGo) Destroy(trailGo);
    }

    private void OnEnable()
    {
        CombatEventBus.OnAttackBeginEvent += BeginTrail;
        CombatEventBus.OnAttackEndEvent += EndTrail;
    }

    private void OnDisable()
    {
        CombatEventBus.OnAttackBeginEvent -= BeginTrail;
        CombatEventBus.OnAttackEndEvent -= EndTrail;
        EndTrail();
    }

    private void SyncPointsFromTransforms()
    {
        if (pointTransforms == null || pointTransforms.Length == 0) return;
        if (points.Length != pointTransforms.Length) points = new Vector3[pointTransforms.Length];

        for (int i = 0; i < pointTransforms.Length; i++)
            if (pointTransforms[i]) points[i] = cacheTM.InverseTransformPoint(pointTransforms[i].position);
    }

    public void BeginTrail()
    {
        if (!enabled || pointTransforms.Length <= 1 || !trailGo) return;

        SyncPointsFromTransforms();
        Initialize((int)(duration / Time.fixedDeltaTime));
        ClearVertices();

        trailGo.SetActive(true);
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
        updateCoroutine = StartCoroutine(PhysicsUpdateRoutine());
    }

    public void EndTrail()
    {
        if (trailGo) trailGo.SetActive(false);
        if (updateCoroutine != null) { StopCoroutine(updateCoroutine); updateCoroutine = null; }
    }

    private void SetVerticesAndCorner()
    {
        int nSP = pointCount + (pointCount * corner);
        Array.Copy(vertices, 0, vertices, nSP, vertices.Length - nSP);
        TransformVertices();

        for (int x = 0; x < pointCount; x++)
        {
            Vector3 a = vertices[x], b = vertices[x + nSP], c = vertices[x + nSP * 2], d = vertices[x + nSP * 3];
            int idx = pointCount + x;

            for (int n = 0; n < corner; n++)
            {
                float t = (n + 1) * toCornerT;
                vertices[idx] = CatmullRomSpline(a, a, b, c, t);
                vertices[idx + nSP] = CatmullRomSpline(a, b, c, d, t);
                idx += pointCount;
            }
        }
    }

    private void SetVertices()
    {
        Array.Copy(vertices, 0, vertices, pointCount, vertices.Length - pointCount);
        TransformVertices();
    }

    private IEnumerator PhysicsUpdateRoutine()
    {
        Action action = corner > 0 ? SetVerticesAndCorner : SetVertices;
        while (true)
        {
            yield return waitForFixedUpdate;
            action();
        }
    }

    private void LateUpdate()
    {
        if (!trailGo || !trailGo.activeSelf) return;

        SyncPointsFromTransforms();
        TransformVertices();

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    private void TransformVertices()
    {
        var matrix = cacheTM.localToWorldMatrix;
        for (int i = 0; i < pointCount; i++) vertices[i] = matrix.MultiplyPoint3x4(points[i]);
    }

    private void ClearVertices()
    {
        TransformVertices();
        for (int i = pointCount; i < vertices.Length; i += pointCount)
            Array.Copy(vertices, 0, vertices, i, pointCount);
    }

    private void Initialize(int segment)
    {
        int curCorner = segment >= 3 ? corner : 0;
        if (lastSegmentCount == segment && pointCount == points.Length && lastCorner == curCorner) return;

        pointCount = points.Length; lastCorner = curCorner; lastSegmentCount = segment;
        if (pointCount <= 1) { mesh.Clear(); return; }

        int segCorner = segment + (segment * curCorner);
        Vector2[] uvs = new Vector2[pointCount * (segCorner + 1)];
        int[] idxs = new int[(Loop ? pointCount : pointCount - 1) * 6 * segCorner];
        float invSeg = 1f / segment, invEnd = 1f / (pointCount - 1);
        toCornerT = 1f / (curCorner + 1);

        int i = -1;
        for (int y = 0; y <= segment; y++)
        {
            for (int x = 0; x < pointCount; x++) uvs[++i] = new Vector2(x * invEnd, y * invSeg);
            if (y == segment) continue;

            for (int n = 0; n < curCorner; n++)
                for (int x = 0; x < pointCount; x++)
                    uvs[++i] = new Vector2(x * invEnd, Mathf.Lerp(y * invSeg, (y + 1) * invSeg, (n + 1) * toCornerT));
        }

        int index = 0, lineCount = Loop ? pointCount : pointCount - 1;
        for (int y = 0; y < segCorner; y++)
        {
            int bg = y * pointCount + (Loop ? pointCount - 1 : 0);
            int nx = y * pointCount + (Loop ? 0 : 1);

            for (int x = 0; x < lineCount; x++)
            {
                idxs[index++] = bg; idxs[index++] = bg + pointCount; idxs[index++] = nx;
                idxs[index++] = nx; idxs[index++] = bg + pointCount; idxs[index++] = nx + pointCount;
                bg = nx++;
            }
        }

        vertices = new Vector3[uvs.Length];
        ClearVertices();
        mesh.vertices = vertices; mesh.uv = uvs; mesh.SetIndices(idxs, MeshTopology.Triangles, 0);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointTransforms == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < pointTransforms.Length; i++)
        {
            if (!pointTransforms[i]) continue;
            Vector3 pos = pointTransforms[i].position;
            Gizmos.DrawSphere(pos, 0.05f);
            if (i > 0 && pointTransforms[i - 1]) Gizmos.DrawLine(pointTransforms[i - 1].position, pos);
        }
    }

    private static Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}