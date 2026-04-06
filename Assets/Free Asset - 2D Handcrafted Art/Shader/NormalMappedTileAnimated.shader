using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
[RequireComponent(typeof(waterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{
    //nået til 12:48 https://www.youtube.com/watch?v=TbGEKpdsmCI&t=196s

    [Header("Mesh Generation")]
    [Range(2, 599)] public int numOfXVertices = 70;
    public float Width  = 10f;
    public float Height = 4f;
    public Material waterMaterial;
    private const int numOfYVertices = 2;

    [Header("Wave Simulation")]
    public float springConstant  = 0.02f;   // stiffness – how fast vertices snap back
    public float damping         = 0.04f;   // energy loss per tick
    public float spread          = 0.05f;   // how much neighbouring vertices pull each other
    public float waveForce       = 1.5f;    // force applied on splash()

    [Header("Gizmo")]
    public Color gizmoColor = Color.cyan;

    // ── mesh ──────────────────────────────────────────────────────────────────
    private Mesh         mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter   meshFilter;
    private EdgeCollider2D edgeCollider;

    // ── vertices ──────────────────────────────────────────────────────────────
    private Vector3[] vertices;
    private int[]     topVertexIndex;   // indices of the top row

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
        edgeCollider           = GetComponent<EdgeCollider2D>();
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
        vertices      = new Vector3[numOfYVertices * numOfXVertices];
        topVertexIndex = new int[numOfXVertices];

        for (int y = 0; y < numOfYVertices; y++)
        {
            for (int x = 0; x < numOfXVertices; x++)
            {
                // xPos spans the Width,  yPos spans the Height
                float xPos = (x / (float)(numOfXVertices - 1)) * Width  - Width  / 2f;
                float yPos = (y / (float)(numOfYVertices - 1)) * Height - Height / 2f;

                vertices[y * numOfXVertices + x] = new Vector3(xPos, yPos, 0f);

                if (y == numOfYVertices - 1)
                    topVertexIndex[x] = y * numOfXVertices + x;
            }
        }

        // ── triangles ─────────────────────────────────────────────────────────
        int[] triangles = new int[(numOfYVertices - 1) * (numOfXVertices - 1) * 6];
        int   index     = 0;

        for (int y = 0; y < numOfYVertices - 1; y++)        // ← was numOfXVertices, wrong
        {
            for (int x = 0; x < numOfXVertices - 1; x++)
            {
                int bottomLeft  = y       * numOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft     = (y + 1) * numOfXVertices + x;
                int topRight    = topLeft  + 1;

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
                (vertices[i].x + Width  / 2f) / Width,
                (vertices[i].y + Height / 2f) / Height);

        // ── assign ────────────────────────────────────────────────────────────
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter   == null) meshFilter   = GetComponent<MeshFilter>();
        if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();

        if (waterMaterial != null)
            meshRenderer.material = waterMaterial;

        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        // ── wave state ────────────────────────────────────────────────────────
        velocities    = new float[numOfXVertices];
        accelerations = new float[numOfXVertices];
        baseHeights   = new float[numOfXVertices];
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
            float displacement  = vertices[topVertexIndex[x]].y - baseHeights[x];
            accelerations[x]    = -springConstant * displacement - damping * velocities[x];
            velocities[x]      += accelerations[x];
            vertices[topVertexIndex[x]].y += velocities[x];
        }

        // 2. neighbour spreading (lateral wave propagation)
        float[] leftDeltas  = new float[numOfXVertices];
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
    private InteractableWater _water;

    private void OnEnable()
    {
        _water = (InteractableWater)target;
        if (!Application.isPlaying)
            _water.GenerateMesh();
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(new VisualElement { style = { height = 8 } });   // spacer

        root.Add(new Button(() =>
        {
            _water.GenerateMesh();
            EditorUtility.SetDirty(_water);
        }) { text = "↺  Regenerate Mesh" });

        root.Add(new Button(() =>
        {
            _water.ResetEdgeCollider();
            EditorUtility.SetDirty(_water);
        }) { text = "↺  Reset Edge Collider" });

        return root;
    }

    // =========================================================================
    //  SCENE GUI – draggable corner handles
    // =========================================================================
    private void OnSceneGUI()
    {
          if (_water == null) { Debug.Log("_water is null"); return; }
           Debug.Log("OnSceneGUI running");
          
        // ── wireframe box ─────────────────────────────────────────────────────
        Handles.color = _water.gizmoColor;
        Vector3 center = _water.transform.position;
        Handles.DrawWireCube(center, new Vector3(_water.Width, _water.Height, 0.1f));

        // ── corner positions ──────────────────────────────────────────────────
        float hw = _water.Width  / 2f;
        float hh = _water.Height / 2f;

        Vector3[] corners = new Vector3[4]
        {
            center + new Vector3(-hw, -hh, 0),  // 0 – bottom-left
            center + new Vector3( hw, -hh, 0),  // 1 – bottom-right
            center + new Vector3(-hw,  hh, 0),  // 2 – top-left
            center + new Vector3( hw,  hh, 0),  // 3 – top-right
        };

        float   handleSize = HandleUtility.GetHandleSize(center) * 0.1f;
        Vector3 snap       = Vector3.one * 0.1f;

        bool changed = false;

        // ── bottom-left ───────────────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        Vector3 newBL = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_water, "Resize Water (BL)");
            ChangeDimensions(ref _water.Width, ref _water.Height,
                             corners[1].x - newBL.x,
                             corners[3].y - newBL.y);
            _water.transform.position += new Vector3(
                (newBL.x - corners[0].x) / 2f,
                (newBL.y - corners[0].y) / 2f, 0);
            changed = true;
        }

        // ── bottom-right ──────────────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        Vector3 newBR = Handles.FreeMoveHandle(corners[1], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_water, "Resize Water (BR)");
            ChangeDimensions(ref _water.Width, ref _water.Height,
                             newBR.x - corners[0].x,
                             corners[3].y - newBR.y);
            _water.transform.position += new Vector3(
                (newBR.x - corners[1].x) / 2f,
                (newBR.y - corners[1].y) / 2f, 0);
            changed = true;
        }

        // ── top-left ──────────────────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        Vector3 newTL = Handles.FreeMoveHandle(corners[2], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_water, "Resize Water (TL)");
            ChangeDimensions(ref _water.Width, ref _water.Height,
                             corners[3].x - newTL.x,
                             newTL.y - corners[0].y);
            _water.transform.position += new Vector3(
                (newTL.x - corners[2].x) / 2f,
                (newTL.y - corners[2].y) / 2f, 0);
            changed = true;
        }

        // ── top-right ─────────────────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        Vector3 newTR = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_water, "Resize Water (TR)");
            ChangeDimensions(ref _water.Width, ref _water.Height,
                             newTR.x - corners[2].x,
                             newTR.y - corners[1].y);
            _water.transform.position += new Vector3(
                (newTR.x - corners[3].x) / 2f,
                (newTR.y - corners[3].y) / 2f, 0);
            changed = true;
        }

        if (changed)
        {
            _water.GenerateMesh();
            EditorUtility.SetDirty(_water);
        }
    }

    // ── helper: clamp dimensions to sane minimums ─────────────────────────────
    private static void ChangeDimensions(ref float width, ref float height,
                                         float newWidth, float newHeight)
    {
        width  = Mathf.Max(0.1f, newWidth);
        height = Mathf.Max(0.1f, newHeight);
    }
}
#endif