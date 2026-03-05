using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity; // <-- added

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

    // input cooldown
    [SerializeField, Min(0f)] private float inputCooldown = 0.3f;
    private float _lastInputTime = -999f;

    [SerializeField] private Color markerColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private int markerOrderOffset = 100;
    [SerializeField] private float markerZOffset = -0.001f;

    [SerializeField] private Slicer slicer;

    // FMOD
    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string chopOneShotEvent; // set in Inspector, ex: event:/Sound Effects/Chop

    private bool AllowCommitEarlyNow => (_verticalPhaseCuts > 0) && (_cutsDone >= _verticalPhaseCuts);

    private bool _isBusy = false;
    private bool _inArea2 = false;
    private SelectableItems _current;

    private bool _cuttingActive = false;
    private int _cutsDone = 0;
    private int _cutsTotal = 0;
    private int _verticalPhaseCuts = 0;
    private int _horizontalDenom = 0;
    private bool _hasCommitted = false;

    private readonly List<float> _vFractions = new(); // vertical line positions in 0..1 (left->right)
    private readonly List<float> _hFractions = new(); // horizontal line positions in 0..1 (bottom->top)

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

    //FOR FIRST THURSDAY 11/6 - DELETE ALL OF START FUNCTION LATER
    private void Start()
    {
        if (inputCamera == null) return;

        Vector3 world = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2D = new Vector2(0, -1);

        RaycastHit2D hit = Physics2D.Raycast(pos2D, Vector2.zero, 0f, itemLayer);
        if (hit.collider == null) return;

        var item = hit.collider.GetComponentInParent<SelectableItems>();
        if (item == null) return;

        StartCoroutine(Co_MoveItemToArea2(item));
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

        if (Input.GetMouseButtonDown(0) && TryConsumeInput())
        {
            TryPickFromArea1();
        }

        if (enableCutting && _cuttingActive && !_isBusy && Input.GetKeyDown(cutKey) && TryConsumeInput())
        {
            PerformCutOnce();
        }

        //WAS ADDED FOR FIRST THURSDAY - CAN DELETE LATER
        //Bug: When clicking the selectableItem, you move to Area2 and it sets RequiredCuts and _cutsDone to 0,
        //So _cutsDone >= RequiredCuts and canCommit = true which transitions to the next cooking step 
        bool canCommit = !_hasCommitted && _current != null && _inArea2
                 && ((_cutsDone >= RequiredCuts));
        Debug.Log(_cutsDone + " and req: " + RequiredCuts + " and can commit: " + canCommit);
        cuttingUI.SetCommitInteractable(canCommit);
        if (canCommit)
        {
            cuttingUI.Show(false);
            CookingManager.instance.Transition();
            gameObject.SetActive(false);
        }
    }

    private bool TryConsumeInput()
    {
        if (inputCooldown <= 0f)
            return true;

        float now = Time.time;
        if (now - _lastInputTime < inputCooldown)
            return false;

        _lastInputTime = now;
        return true;
    }

    private void TryPickFromArea1()
    {
        if (inputCamera == null) return;

        Vector3 world = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2D = new Vector2(world.x, world.y);

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

        item.MarkSelected(true);

        //The 2 commented lines below were commented for GDC build because of changes made to CameraMover
        // if (cameraController != null && area2ViewPoint != null)
        //     yield return cameraController.MoveTo(area2ViewPoint);

        _inArea2 = true;
        UpdateCuttingUI();

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
        Debug.Log("" + _cutsTotal);
        _cutsDone = 0;
        _verticalPhaseCuts = Mathf.Clamp(item.RoughChopSlices, 0, RequiredCuts);
        _horizontalDenom = Mathf.Max(1, _cutsTotal - _verticalPhaseCuts);
        _cuttingActive = (RequiredCuts > 0);
        _hasCommitted = false;

        _vFractions.Clear();
        _hFractions.Clear();

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

        // FMOD: play one chop per successful cut
        PlayChopSfx();

        int cutIndex = _cutsDone; // 1-based

        if (cutIndex <= _verticalPhaseCuts)
        {
            int vDen = _verticalPhaseCuts + 1;
            float frac = (float)cutIndex / Mathf.Max(1, vDen);
            _vFractions.Add(frac);
            DrawVerticalMarkerAtFraction(_current, frac);
        }
        else
        {
            int hIndex = cutIndex - _verticalPhaseCuts;
            float frac = (float)hIndex / Mathf.Max(1, _horizontalDenom);
            _hFractions.Add(frac);
            DrawHorizontalMarkerAtFraction(_current, frac);
        }

        UpdateCuttingUI();

        if (_cutsDone >= RequiredCuts)
            _cuttingActive = false;

        UpdateCuttingUI();
    }

    private void PlayChopSfx()
    {
        if (string.IsNullOrEmpty(chopOneShotEvent)) return;

        Vector3 pos = _current != null ? _current.transform.position : transform.position;
        RuntimeManager.PlayOneShot(chopOneShotEvent, pos);
    }

    private void DrawVerticalMarkerAtFraction(SelectableItems item, float fraction01)
    {
        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        Bounds b = sr.bounds;
        float x = Mathf.Lerp(b.min.x, b.max.x, Mathf.Clamp01(fraction01));
        float z = sr.transform.position.z + markerZOffset;

        Vector3 p1 = new Vector3(x, b.min.y, z);
        Vector3 p2 = new Vector3(x, b.max.y, z);

        CreateMarker(sr, p1, p2);
    }

    private void DrawHorizontalMarkerAtFraction(SelectableItems item, float fraction01)
    {
        var sr = item.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        Bounds b = sr.bounds;
        float y = Mathf.Lerp(b.min.y, b.max.y, Mathf.Clamp01(fraction01));
        float z = sr.transform.position.z + markerZOffset;

        Vector3 p1 = new Vector3(b.min.x, y, z);
        Vector3 p2 = new Vector3(b.max.x, y, z);

        CreateMarker(sr, p1, p2);
    }

    private void CreateMarker(SpriteRenderer targetSr, Vector3 p1, Vector3 p2)
    {
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

        lr.sortingLayerID = targetSr.sortingLayerID;
        lr.sortingOrder = targetSr.sortingOrder + markerOrderOffset;

        go.transform.SetParent(_current.transform, worldPositionStays: true);

        _markers.Add(new MarkerRec { lr = lr, targetSr = targetSr });
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

    public void CommitSlice()
    {
        if (_current == null || slicer == null) return;

        bool canCommit = !_hasCommitted && _inArea2
                     && (AllowCommitEarlyNow || (_cutsDone >= RequiredCuts));
        if (!canCommit) return;

        ClearMarkers();

        slicer.SliceGridItem(_current, _vFractions, _hFractions);

        _hasCommitted = true;
        UpdateCuttingUI();
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