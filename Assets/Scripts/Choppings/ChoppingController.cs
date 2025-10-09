using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingController : MonoBehaviour
{
    [SerializeField] private Transform area1ViewPoint;
    [SerializeField] private Transform area2ViewPoint;
    [SerializeField] private Transform targetSpotInArea2;

    [SerializeField] private CameraMover cameraController;

    [SerializeField] private Camera inputCamera; // 用于屏幕坐标 -> 世界坐标，默认 Main Camera
    [SerializeField] private LayerMask itemLayer = ~0; // 建议只勾选“Item”层

    [SerializeField, Range(0.05f, 5f)] private float itemMoveDuration = 0.35f;
    [SerializeField] private AnimationCurve itemMoveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _isBusy = false;
    private SelectableItems _current;

    private void Awake()
    {
        if (inputCamera == null) inputCamera = Camera.main;
        if (cameraController == null) cameraController = FindObjectOfType<CameraMover>();

        // 基本断言，便于早期发现引用遗漏
        if (area1ViewPoint == null) Debug.LogWarning("[GameFlowController] area1ViewPoint 未设置。");
        if (area2ViewPoint == null) Debug.LogWarning("[GameFlowController] area2ViewPoint 未设置。");
        if (targetSpotInArea2 == null) Debug.LogWarning("[GameFlowController] targetSpotInArea2 未设置。");
        if (cameraController == null) Debug.LogWarning("[GameFlowController] cameraController 未设置。");
        if (inputCamera == null) Debug.LogWarning("[GameFlowController] inputCamera 未设置。");
    }

    private void OnValidate()
    {
        if (itemMoveDuration < 0.05f) itemMoveDuration = 0.05f;
        if (itemMoveEase == null) itemMoveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
        if (inputCamera == null) inputCamera = Camera.main;
        if (cameraController == null) cameraController = FindObjectOfType<CameraMover>();
    }

    private void Update()
    {
        if (_isBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPickFromArea1();
        }
    }

    private void TryPickFromArea1()
    {
        if (inputCamera == null) return;

        Vector3 world = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2D = new Vector2(world.x, world.y);

        // 用零长度射线做点选并可带 LayerMask
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

        // 1) 标记选中
        item.MarkSelected(true);

        // 2) 相机从 Area1 → Area2（如果有设置）
        if (cameraController != null && area2ViewPoint != null)
            yield return cameraController.MoveTo(area2ViewPoint);

        // 3) 物体平滑移动到目标点
        if (targetSpotInArea2 != null)
            yield return StartCoroutine(Co_SmoothMove(item.transform, targetSpotInArea2.position, itemMoveDuration, itemMoveEase));

        _current = item;
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
    }
}
