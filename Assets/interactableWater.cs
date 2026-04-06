using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
[RequireComponent(typeof(waterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{
    [Header("Mesh Generation")]
    [Range(2, 599)] public int numOfXVertices = 70;
    public float Width = 10f;
    public float Height = 4f;
    public Material waterMaterial;
    private const int numOfYVertices = 2;

    [Header("Wave Simulation")]
    public float springConstant = 0.02f;   // stiffness – how fast vertices snap back
    public float damping = 0.04f;   // energy loss per tick
    public float spread = 0.05f;   // how much neighbouring vertices pull each other
    public float waveForce = 1.5f;    // force applied on splash()

    [Header("Gizmo")]
    public Color gizmoColor = Color.cyan;

    // ── mesh ──────────────────────────────────────────────────────────────────
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private EdgeCollider2D edgeCollider;

    // ── vertices ──────────────────────────────────────────────────────────────
    private Vector3[] vertices;
    private int[] topVertexIndex;   // indices of the top row

    // ── wave state ────────────────────────────────────────────────────────────
    private float[] velocities;         // vertical velocity per top vertex
    private float[] accelerations;     // vertical acceleration per top vertex
    private float[] baseHeights;        // rest position Y per top vertex

    // =========================================================================
    private void Start()
    {
        GenerateMesh();
    }

    private void Reset()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
    }

    // ── update ─────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (velocities == null || vertices == null) return;

        SimulateWaves();
        UpdateMeshAndCollider();
    }

    // =========================================================================
    //  MESH GENERATION
    // =========================================================================
    public void GenerateMesh()
    {
        mesh = new Mesh();

        // ── vertices ──────────────────────────────────────────────────────────
        vertices = new Vector3[numOfYVertices * numOfXVertices];
        topVertexIndex = new int[numOfXVertices];

        for (int y = 0; y < numOfYVertices; y++)
        {
            for (int x = 0; x < numOfXVertices; x++)
            {
                // xPos spans the Width,  yPos spans the Height
                float xPos = (x / (float)(numOfXVertices - 1)) * Width - Width / 2f;
                float yPos = (y / (float)(numOfYVertices - 1)) * Height - Height / 2f;

                vertices[y * numOfXVertices + x] = new Vector3(xPos, yPos, 0f);

                if (y == numOfYVertices - 1)
                    topVertexIndex[x] = y * numOfXVertices + x;
            }
        }

        // ── triangles ─────────────────────────────────────────────────────────
        int[] triangles = new int[(numOfYVertices - 1) * (numOfXVertices - 1) * 6];
        int index = 0;

        for (int y = 0; y < numOfYVertices - 1; y++)        // ← was numOfXVertices, wrong
        {
            for (int x = 0; x < numOfXVertices - 1; x++)
            {
                int bottomLeft = y * numOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = (y + 1) * numOfXVertices + x;
                int topRight = topLeft + 1;

                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
            }
        }

        // ── UV ────────────────────────────────────────────────────────────────
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            uv[i] = new Vector2(
                (vertices[i].x + Width / 2f) / Width,
                (vertices[i].y + Height / 2f) / Height);

        // ── assign ────────────────────────────────────────────────────────────
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();

        if (waterMaterial != null)
            meshRenderer.material = waterMaterial;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        // ── wave state ────────────────────────────────────────────────────────
        velocities = new float[numOfXVertices];
        accelerations = new float[numOfXVertices];
        baseHeights = new float[numOfXVertices];
        for (int x = 0; x < numOfXVertices; x++)
            baseHeights[x] = vertices[topVertexIndex[x]].y;

        ResetEdgeCollider();
    }

    // =========================================================================
    //  WAVE SIMULATION  (simple spring model)
    // =========================================================================
    private void SimulateWaves()
    {
        // 1. spring forces
        for (int x = 0; x < numOfXVertices; x++)
        {
            float displacement = vertices[topVertexIndex[x]].y - baseHeights[x];
            accelerations[x] = -springConstant * displacement - damping * velocities[x];
            velocities[x] += accelerations[x];
            vertices[topVertexIndex[x]].y += velocities[x];
        }

        // 2. neighbour spreading (lateral wave propagation)
        float[] leftDeltas = new float[numOfXVertices];
        float[] rightDeltas = new float[numOfXVertices];

        for (int x = 0; x < numOfXVertices; x++)
        {
            if (x > 0)
            {
                leftDeltas[x] = spread * (vertices[topVertexIndex[x]].y - vertices[topVertexIndex[x - 1]].y);
                velocities[x - 1] += leftDeltas[x];
            }
            if (x < numOfXVertices - 1)
            {
                rightDeltas[x] = spread * (vertices[topVertexIndex[x]].y - vertices[topVertexIndex[x + 1]].y);
                velocities[x + 1] += rightDeltas[x];
            }
        }
    }

    private void UpdateMeshAndCollider()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        ResetEdgeCollider();
    }

    // =========================================================================
    //  PUBLIC API
    // =========================================================================

    /// <summary>
    /// Splash at world-space X position. Call this from waterTriggerHandler.
    /// </summary>
    public void Splash(float worldX, float force)
    {
        if (velocities == null) return;

        // find closest top vertex
        int closest = 0;
        float minDist = float.MaxValue;
        for (int x = 0; x < numOfXVertices; x++)
        {
            float dist = Mathf.Abs(transform.TransformPoint(vertices[topVertexIndex[x]]).x - worldX);
            if (dist < minDist) { minDist = dist; closest = x; }
        }
        velocities[closest] += force * waveForce;
    }

    /// <summary>
    /// Rebuilds the EdgeCollider2D to match the current top-row vertices.
    /// </summary>
    public void ResetEdgeCollider()
    {
        if (edgeCollider == null)
            edgeCollider = GetComponent<EdgeCollider2D>();

        edgeCollider.isTrigger = true;

        Vector2[] points = new Vector2[numOfXVertices];
        for (int x = 0; x < numOfXVertices; x++)
        {
            Vector3 v = vertices[topVertexIndex[x]];
            points[x] = new Vector2(v.x, v.y);
        }
        edgeCollider.SetPoints(new System.Collections.Generic.List<Vector2>(points));
    }

    // =========================================================================
    //  GIZMO
    // =========================================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(center, new Vector3(Width, Height, 0.1f));
    }
}

// =============================================================================
//  EDITOR
// =============================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(InteractableWater))]
public class InteractableWaterEditor : Editor
{
    private void OnEnable()
    {
        var water = (InteractableWater)target;
        if (!Application.isPlaying)
            water.GenerateMesh();
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(new VisualElement { style = { height = 8 } });   // spacer

        root.Add(new Button(() =>
        {
            var water = (InteractableWater)target;
            water.GenerateMesh();
            EditorUtility.SetDirty(water);
        })
        { text = "↺  Regenerate Mesh" });

        root.Add(new Button(() =>
        {
            var water = (InteractableWater)target;
            water.ResetEdgeCollider();
            EditorUtility.SetDirty(water);
        })
        { text = "↺  Reset Edge Collider" });

        return root;
    }
}
#endif