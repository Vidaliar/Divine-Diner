using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingController : MonoBehaviour
{
    [SerializeField] private Transform area1ViewPoint;
    [SerializeField] private Transform area2ViewPoint;
    [SerializeField] private Transform targetSpotInArea2;

    [SerializeField] private CameraMover cameraController;

    [SerializeField] private Camera inputCamera; //  world position, Main Camera as defult
    [SerializeField] private LayerMask itemLayer = ~0; // only layer Items

    [SerializeField, Range(0.05f, 5f)] private float itemMoveDuration = 0.35f;
    [SerializeField] private AnimationCurve itemMoveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("CHOPPINGS")]
    [SerializeField] private bool enableCutting = true;
    [SerializeField] private KeyCode cutKey = KeyCode.Space;
    [SerializeField, Min(0.001f)] private float markerWidth = 0.03f; // the width for marker (world position)
    [SerializeField] private Material markerMaterial;                 // can leave it defult
    [SerializeField] private CuttingUIController cuttingUI;           // UI

    [SerializeField] private Color markerColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private int markerOrderOffset = 100;
    [SerializeField] private float markerZOffset = -0.001f;

    private bool _isBusy = false;
    private bool _inArea2 = false;
    private SelectableItems _current;

    private bool _cuttingActive = false;
    private int _cutsDone = 0;
    private int _cutsTotal = 0;

    private int RequiredCuts => Mathf.Max(0, _cutsTotal - 1);


    private struct MarkerRec
    {
        public LineRenderer lr;
        public SpriteRenderer targetSr;
    }
    private readonly List<MarkerRec> _markers = new List<MarkerRec>();

    private void Awake()
    {
        if (inputCamera == null) inputCamera = Camera.main;
        if (cameraController == null) cameraController = FindObjectOfType<CameraMover>();

        _inArea2 = false;
        if (cuttingUI != null) cuttingUI.Show(false);

        if (area1ViewPoint == null) Debug.LogWarning("[GameFlowController] area1ViewPoint Not Set Yet.");
        if (area2ViewPoint == null) Debug.LogWarning("[GameFlowController] area2ViewPoint Not Set Yet.");
        if (targetSpotInArea2 == null) Debug.LogWarning("[GameFlowController] targetSpotInArea2 Not Set Yet.");
        if (cameraController == null) Debug.LogWarning("[GameFlowController] cameraController Not Set Yet.");
        if (inputCamera == null) Debug.LogWarning("[GameFlowController] inputCamera Not Set Yet.");
    }

    private void OnValidate()
    {
        if (itemMoveDuration < 0.05f) itemMoveDuration = 0.05f;
        if (itemMoveEase == null) itemMoveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
        if (inputCamera == null) inputCamera = Camera.main;
        if (cameraController == null) cameraController = FindObjectOfType<CameraMover>();

        if (markerWidth < 0.001f) markerWidth = 0.001f;
        if (markerOrderOffset < 1) markerOrderOffset = 1;
    }

    private void Update()
    {
        if (_isBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPickFromArea1();
        }

        if (enableCutting && _cuttingActive && !_isBusy && Input.GetKeyDown(cutKey))
        {
            PerformCutOnce();
        }
    }

    private void TryPickFromArea1()
    {
        if (inputCamera == null) return;

        Vector3 world = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2D = new Vector2(world.x, world.y);

        // use raycast to choose, also can work with LayerMask
        RaycastHit2D hit = Physics2D.Raycast(pos2D, Vector2.zero, 0f, itemLayer);
        if (hit.collider == null) return;

        var item = hit.collider.GetComponentInParent<SelectableItems>();
        if (item == null) return;

        StartCoroutine(Co_MoveItemToArea2(item));
    }

    private IEnumerator Co_MoveItemToArea2(SelectableItems item)
    {
        _isBusy = true;

        if (_current != null && _current != item)
            _current.MarkSelected(false);

        EndCuttingSession();

        if (_current != null && _current != item)
            _current.MarkSelected(false);

        // choose
        item.MarkSelected(true);

        // camera move by point placed
        if (cameraController != null && area2ViewPoint != null)
            yield return cameraController.MoveTo(area2ViewPoint);

        _inArea2 = true;
        UpdateCuttingUI();

        // items move
        if (targetSpotInArea2 != null)
            yield return StartCoroutine(Co_SmoothMove(item.transform, targetSpotInArea2.position, itemMoveDuration, itemMoveEase));

        _current = item;

        if (enableCutting && _current != null)
            BeginCuttingSession(_current);

        _isBusy = false;
    }

    private IEnumerator Co_SmoothMove(Transform tr, Vector3 targetPos, float duration, AnimationCurve curve)
    {
        Vector3 start = tr.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = curve != null ? curve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            tr.position = Vector3.LerpUnclamped(start, targetPos, k);
            yield return null;
        }
        tr.position = targetPos;
    }

    public void BackToArea1View()
    {
        if (_isBusy) return;
        if (cameraController != null && area1ViewPoint != null)
            cameraController.MoveTo(area1ViewPoint);

        _inArea2 = false;
        EndCuttingSession();
        if (cuttingUI != null)
            cuttingUI.Show(false);
    }
    private void BeginCuttingSession(SelectableItems item)
    {
        _cutsTotal = Mathf.Max(1, item.TotalSlices);
        _cutsDone = 0;
        _cuttingActive = (RequiredCuts > 0);

        // Clear chopping UI first
        ClearMarkers();
        UpdateCuttingUI();
    }

    private void EndCuttingSession()
    {
        _cuttingActive = false;
        _cutsDone = 0;
        _cutsTotal = 0;
        ClearMarkers();
        UpdateCuttingUI();
    }

    private void UpdateCuttingUI()
    {
        if (cuttingUI == null) return;

        bool show = enableCutting
                    && _current != null
                    && (_inArea2);

        cuttingUI.Show(show);

        int totalPieces = Mathf.Max(1, _cutsTotal);
        int currentPieces = Mathf.Clamp(_cutsDone + 1, 1, totalPieces);
        cuttingUI.UpdateProgress(currentPieces, totalPieces);
    }

    private void PerformCutOnce()
    {
        if (!_cuttingActive || _current == null) return;

        if (_cutsDone >= RequiredCuts) return;

        _cutsDone++;

        //draw a chop line, from left to right
        DrawMarkerAtFraction(_current, (float)_cutsDone / _cutsTotal);

        // Update UI
        UpdateCuttingUI();

        // end the chop mode when all completed
        if (_cutsDone >= _cutsTotal)
        {
            _cuttingActive = false;
            UpdateCuttingUI();
        }
    }

    private void DrawMarkerAtFraction(SelectableItems item, float fraction01)
    {
        // 0 < fraction01 <=1£¬count by the world bounds of SpriteRenderer
        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        Bounds b = sr.bounds;
        float x = Mathf.Lerp(b.min.x, b.max.x, Mathf.Clamp01(fraction01));
        float z = sr.transform.position.z + markerZOffset;
        Vector3 p1 = new Vector3(x, b.min.y, sr.transform.position.z);
        Vector3 p2 = new Vector3(x, b.max.y, sr.transform.position.z);

        var go = new GameObject($"CutMarker_{_cutsDone}/{_cutsTotal}");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, p1);
        lr.SetPosition(1, p2);
        lr.startWidth = markerWidth;
        lr.endWidth = markerWidth;
        lr.numCapVertices = 2;
        lr.numCornerVertices = 2;

        lr.material = GetOrCreateMarkerMaterial();
        lr.startColor = markerColor;
        lr.endColor = markerColor;

        lr.sortingLayerID = sr.sortingLayerID;
        lr.sortingOrder = sr.sortingOrder + 1;

        // make line follow the object
        go.transform.SetParent(item.transform, worldPositionStays: true);

        _markers.Add(new MarkerRec { lr = lr, targetSr = sr });
    }

    private Material _runtimeMarkerMat;
    private Material GetOrCreateMarkerMaterial()
    {
        if (markerMaterial != null) return markerMaterial;

        if (_runtimeMarkerMat == null)
        {
            Shader sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Unlit/Color");

            _runtimeMarkerMat = new Material(sh != null ? sh : Shader.Find("Sprites/Default"));
            if (_runtimeMarkerMat.HasProperty("_Color")) _runtimeMarkerMat.SetColor("_Color", markerColor);
            if (_runtimeMarkerMat.HasProperty("_BaseColor")) _runtimeMarkerMat.SetColor("_BaseColor", markerColor);
        }
        return _runtimeMarkerMat;
    }
    private void RefreshMarkersSorting()
    {
        if (_markers.Count == 0) return;
        for (int i = 0; i < _markers.Count; i++)
        {
            var m = _markers[i];
            if (m.lr == null || m.targetSr == null) continue;

            m.lr.sortingLayerID = m.targetSr.sortingLayerID;
            m.lr.sortingOrder = m.targetSr.sortingOrder + markerOrderOffset;

            var pos0 = m.lr.GetPosition(0);
            var pos1 = m.lr.GetPosition(1);
            float z = m.targetSr.transform.position.z + markerZOffset;
            if (!Mathf.Approximately(pos0.z, z) || !Mathf.Approximately(pos1.z, z))
            {
                pos0.z = z; pos1.z = z;
                m.lr.SetPosition(0, pos0);
                m.lr.SetPosition(1, pos1);
            }
        }
    }


    private void ClearMarkers()
    {
        foreach (var m in _markers)
        {
            if (m.lr != null) Destroy(m.lr.gameObject);
        }
        _markers.Clear();
    }
}
