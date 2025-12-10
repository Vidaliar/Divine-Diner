using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer : MonoBehaviour
{
    public enum PiecePhysicsMode
    {
        None,           // visual only
        StaticCollider, // BoxCollider2D only
        KinematicFrozen,// Rigidbody2D(Kinematic) + FreezeAll
        DynamicExplode  // Rigidbody2D(Dynamic) + optional impulse
    }

    [Header("Target & Defaults")]
    [SerializeField] private Transform defaultRoot;
    [SerializeField] private int sortingOrderOffset = 2;

    [Header("Physics Mode")]
    [SerializeField] private PiecePhysicsMode physicsMode = PiecePhysicsMode.None;
    [SerializeField] private bool disableOriginalColliders = true;

    [Header("Dynamic/Kinematic Settings")]
    [SerializeField] private float pieceMass = 0.1f;
    [SerializeField] private float gravityScale = 0f;
    [SerializeField] private PhysicsMaterial2D pieceMaterial;
    [SerializeField] private bool useExplodeImpulse = false; // only for DynamicExplode
    [SerializeField] private float explodeForce = 1.5f;
    [SerializeField] private float explodeTorque = 2f;
    [SerializeField] private float explodeSpread = 0.4f;

    [Header("Post-slice Separation (non-physics)")]
    [SerializeField] private bool applySeparationOffset = true;
    [SerializeField] private float separationSpanX = 0.12f;         // total X spread
    [SerializeField] private float separationSpanY = 0.08f;         // total Y spread
    [SerializeField] private float separationJitter = 0.00f;        // small random jitter
    [SerializeField] private bool tweenSeparation = false;
    [SerializeField, Range(0.01f, 1f)] private float separationDuration = 0.15f;
    [SerializeField] private AnimationCurve separationEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual")]
    [SerializeField] private float sliceGap = 0.01f;                // applies on both width & height
    [SerializeField] private bool keepOriginalDisabled = true;

    private Sprite _squareMaskSprite;
    private GameObject _lastSlicesRoot;

    // ---------------- Public API (legacy vertical-only kept for compatibility) ----------------

    public List<GameObject> SliceItem(SelectableItems item)
    {
        var result = new List<GameObject>();
        if (item == null) return result;

        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return result;

        // Fallback: legacy vertical-only equal slicing by TotalSlices
        int total = Mathf.Max(1, item.TotalSlices);
        return SliceSpriteVerticalEqual(sr, total, item.transform);
    }

    // NEW: driven by drawn vertical+horizontal lines (fractions in 0..1)
    public List<GameObject> SliceGridItem(SelectableItems item, IList<float> verticalFractions01, IList<float> horizontalFractions01)
    {
        var result = new List<GameObject>();
        if (item == null) return result;
        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return result;

        var pieces = SliceSpriteGrid(sr, verticalFractions01, horizontalFractions01, item.transform);

        if (disableOriginalColliders)
        {
            foreach (var col in item.GetComponentsInChildren<Collider2D>(includeInactive: true))
                col.enabled = false;
        }
        return pieces;
    }

    // ---------------- Grid slicer (core) ----------------

    public List<GameObject> SliceSpriteGrid(SpriteRenderer sourceSr, IList<float> vFractions01, IList<float> hFractions01, Transform parentRoot = null)
    {
        var created = new List<GameObject>();
        if (sourceSr == null) return created;

        EnsureMaskSprite();

        // Build column and row edges: add 0 and 1, sort & unique
        var xEdges = BuildEdgesSorted(vFractions01);
        var yEdges = BuildEdgesSorted(hFractions01);

        int cols = Mathf.Max(1, xEdges.Count - 1);
        int rows = Mathf.Max(1, yEdges.Count - 1);
        if (cols * rows <= 1)
        {
            // Nothing to split into (<=1 piece)
            return created;
        }

        Bounds b = sourceSr.bounds;
        Transform root = parentRoot != null ? parentRoot : (defaultRoot != null ? defaultRoot : sourceSr.transform);

        int layerId = sourceSr.sortingLayerID;
        int orderBase = sourceSr.sortingOrder + sortingOrderOffset;

        var group = new GameObject($"{sourceSr.gameObject.name}_GridSlicesRoot");
        group.transform.SetPositionAndRotation(sourceSr.transform.position, sourceSr.transform.rotation);
        group.transform.localScale = sourceSr.transform.lossyScale;
        group.transform.SetParent(root, worldPositionStays: true);

        // For non-physics separation tween
        var maskRoots = new List<Transform>();
        var startPos = new List<Vector3>();
        var endPos = new List<Vector3>();

        float gap = Mathf.Max(0f, sliceGap);

        for (int r = 0; r < rows; r++)
        {
            float y0 = Mathf.Lerp(b.min.y, b.max.y, yEdges[r]);
            float y1 = Mathf.Lerp(b.min.y, b.max.y, yEdges[r + 1]);
            float cellH = Mathf.Max(0.0001f, (y1 - y0) - gap);
            float cy = (y0 + y1) * 0.5f;

            for (int c = 0; c < cols; c++)
            {
                float x0 = Mathf.Lerp(b.min.x, b.max.x, xEdges[c]);
                float x1 = Mathf.Lerp(b.min.x, b.max.x, xEdges[c + 1]);
                float cellW = Mathf.Max(0.0001f, (x1 - x0) - gap);
                float cx = (x0 + x1) * 0.5f;

                Vector3 center = new Vector3(cx, cy, sourceSr.transform.position.z);

                // --- Mask root for this cell ---
                var maskGO = new GameObject($"CellMask_{r}_{c}");
                var mask = maskGO.AddComponent<SpriteMask>();
                mask.sprite = _squareMaskSprite;
                mask.isCustomRangeActive = false;

                maskGO.transform.position = center;
                maskGO.transform.rotation = sourceSr.transform.rotation;
                maskGO.transform.localScale = new Vector3(cellW, cellH, 1f);
                maskGO.transform.SetParent(group.transform, worldPositionStays: true);

                // --- Visible piece ---
                var pieceGO = new GameObject($"CellPiece_{r}_{c}");
                var pieceSr = pieceGO.AddComponent<SpriteRenderer>();
                pieceSr.sprite = sourceSr.sprite;
                pieceSr.color = sourceSr.color;
                pieceSr.flipX = sourceSr.flipX;
                pieceSr.flipY = sourceSr.flipY;
                pieceSr.sortingLayerID = layerId;
                pieceSr.sortingOrder = orderBase + (r * cols + c);
                pieceSr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                pieceGO.transform.SetPositionAndRotation(sourceSr.transform.position, sourceSr.transform.rotation);
                pieceGO.transform.localScale = sourceSr.transform.lossyScale;
                pieceGO.transform.SetParent(maskGO.transform, worldPositionStays: true);

                // Physics per mode
                ApplyPhysics(pieceGO, cellW, cellH);

                created.Add(maskGO);
                maskRoots.Add(maskGO.transform);
                startPos.Add(maskGO.transform.position);
            }
        }

        // Non-physics separation (spread by (col,row) within spans)
        if (applySeparationOffset && maskRoots.Count > 0 && (separationSpanX != 0f || separationSpanY != 0f))
        {
            endPos.Clear();
            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                float tY = (rows <= 1) ? 0.5f : (float)r / (rows - 1); // 0..1
                float offY = Mathf.Lerp(-separationSpanY * 0.5f, separationSpanY * 0.5f, tY);

                for (int c = 0; c < cols; c++, index++)
                {
                    float tX = (cols <= 1) ? 0.5f : (float)c / (cols - 1); // 0..1
                    float offX = Mathf.Lerp(-separationSpanX * 0.5f, separationSpanX * 0.5f, tX);

                    float j = (separationJitter > 0f) ? Random.Range(-separationJitter, separationJitter) : 0f;

                    var p = startPos[index];
                    endPos.Add(new Vector3(p.x + offX, p.y + offY + j, p.z));
                }
            }

            if (tweenSeparation && separationDuration > 0.01f)
                StartCoroutine(Co_ApplySeparation(maskRoots, startPos, endPos));
            else
                for (int i = 0; i < maskRoots.Count; i++) maskRoots[i].position = endPos[i];
        }

        if (keepOriginalDisabled) sourceSr.enabled = false;

        _lastSlicesRoot = group;
        return created;
    }

    // ---------------- Helpers ----------------

    // Legacy vertical equal slices (kept for compatibility with SliceItem)
    private List<GameObject> SliceSpriteVerticalEqual(SpriteRenderer sourceSr, int totalSlices, Transform parentRoot)
    {
        // Convert equal N vertical slices into a grid call with N-1 internal vertical lines, 0 horizontal lines
        var v = new List<float>();
        for (int i = 1; i < totalSlices; i++) v.Add((float)i / totalSlices); // internal cuts
        var h = new List<float>(); // none
        return SliceSpriteGrid(sourceSr, v, h, parentRoot);
    }

    private List<float> BuildEdgesSorted(IList<float> fractions01)
    {
        var list = new List<float> { 0f, 1f };
        if (fractions01 != null)
        {
            for (int i = 0; i < fractions01.Count; i++)
            {
                float f = Mathf.Clamp01(fractions01[i]);
                if (f <= 0f || f >= 1f) continue; // exclude edges
                list.Add(f);
            }
        }
        list.Sort();

        // dedupe with small epsilon
        const float eps = 1e-4f;
        var uniq = new List<float>(list.Count);
        float last = -10f;
        foreach (var v in list)
        {
            if (uniq.Count == 0 || Mathf.Abs(v - last) > eps)
            {
                uniq.Add(v);
                last = v;
            }
        }
        return uniq;
    }

    private void ApplyPhysics(GameObject pieceGO, float w, float h)
    {
        switch (physicsMode)
        {
            case PiecePhysicsMode.None:
                // no collider, no rigidbody
                break;

            case PiecePhysicsMode.StaticCollider:
                {
                    var col = pieceGO.AddComponent<BoxCollider2D>();
                    col.sharedMaterial = pieceMaterial;
                    col.size = new Vector2(w, h);
                    break;
                }

            case PiecePhysicsMode.KinematicFrozen:
                {
                    var col = pieceGO.AddComponent<BoxCollider2D>();
                    col.sharedMaterial = pieceMaterial;
                    col.size = new Vector2(w, h);

                    var rb = pieceGO.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.mass = pieceMass;
                    rb.gravityScale = gravityScale;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    break;
                }

            case PiecePhysicsMode.DynamicExplode:
                {
                    var col = pieceGO.AddComponent<BoxCollider2D>();
                    col.sharedMaterial = pieceMaterial;
                    col.size = new Vector2(w, h);

                    var rb = pieceGO.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.mass = pieceMass;
                    rb.gravityScale = gravityScale;

                    if (useExplodeImpulse)
                    {
                        // NOTE: for grid we skip auto-impulse by default; toggle useExplodeImpulse to enable.
                        Vector2 impulse = new Vector2(Random.Range(-explodeSpread, explodeSpread), 1f).normalized * explodeForce;
                        rb.AddForce(impulse, ForceMode2D.Impulse);
                        rb.AddTorque(Random.Range(-explodeTorque, explodeTorque), ForceMode2D.Impulse);
                    }
                    break;
                }
        }
    }

    public void ClearLastSlices()
    {
        if (_lastSlicesRoot != null)
        {
            Destroy(_lastSlicesRoot);
            _lastSlicesRoot = null;
        }
    }

    public void RestoreOriginal(SpriteRenderer sourceSr)
    {
        if (sourceSr != null) sourceSr.enabled = true;
        ClearLastSlices();
    }

    public void RestoreOriginal(SelectableItems item)
    {
        if (item == null) { ClearLastSlices(); return; }
        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
        ClearLastSlices();
    }

    private IEnumerator Co_ApplySeparation(List<Transform> masks, List<Vector3> from, List<Vector3> to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, separationDuration);
            float k = separationEase != null ? separationEase.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            for (int i = 0; i < masks.Count; i++)
                masks[i].position = Vector3.LerpUnclamped(from[i], to[i], k);
            yield return null;
        }
        for (int i = 0; i < masks.Count; i++) masks[i].position = to[i];
    }

    private void EnsureMaskSprite()
    {
        if (_squareMaskSprite != null) return;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _squareMaskSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        _squareMaskSprite.name = "SlicerMask_1x1_PPU1";
    }
}
