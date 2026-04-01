using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
================================================================================
CameraMover
================================================================================
Purpose
- A lightweight camera mover for Unity 2D.
- Supports:
    1) single straight move
    2) single quadratic Bezier move
    3) configured multi-segment path playback
- Only X/Y are moved. Camera Z is preserved.

Hierarchy for configured path
- CameraPath
    - Points
        - Waypoint1
        - Waypoint2
        - Waypoint3
        - ...
    - Segments
        - Segment_1_2
            - Control
        - Segment_2_3
            - Control
        - ...

Rules
- The child order under Points defines waypoint order.
- The child order under Segments defines segment order.
- Segments[i] controls Points[i] -> Points[i + 1].
- If a segment has no child, that segment falls back to straight movement.

API
- Coroutine MoveTo(Transform target)
    * Straight move by default, or uses defaultControlPoint if assigned.
- Coroutine MoveTo(Transform target, Transform controlPoint)
    * Single quadratic Bezier move when controlPoint is not null.
- Coroutine PlayWholeConfiguredPath()
    * Plays the whole configured path from point 0 to the last point.
- Coroutine PlayConfiguredPath(int startPointIndex, int endPointIndex)
    * Plays from Points[startPointIndex] to Points[endPointIndex].
- void SnapToPoint(int pointIndex)
    * Instantly places the camera at a configured point.
================================================================================
*/
public class CameraMover : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Move Settings")]
    [SerializeField, Range(0.1f, 5f)] private float moveDuration = 1.0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional Default Bezier Control For Single Move")]
    [SerializeField] private Transform defaultControlPoint;

    [Header("Configured Path")]
    [SerializeField] private Transform pointsRoot;
    [SerializeField] private Transform segmentsRoot;
    [SerializeField] private bool snapToStartPointBeforePlay = false;

    private Coroutine _movingCo;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null)
            Debug.LogError("[CameraMover] Target camera is missing.");
    }

    private void OnValidate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (moveDuration < 0.1f) moveDuration = 0.1f;
        if (ease == null) ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public void StopMoving()
    {
        if (_movingCo != null)
        {
            StopCoroutine(_movingCo);
            _movingCo = null;
        }
    }

    public Coroutine MoveTo(Transform target)
    {
        return MoveTo(target, defaultControlPoint);
    }

    public Coroutine MoveTo(Transform target, Transform controlPoint)
    {
        if (targetCamera == null || target == null) return null;

        StopMoving();

        Vector3? controlPos = null;
        if (controlPoint != null)
            controlPos = controlPoint.position;

        _movingCo = StartCoroutine(Co_MoveTo(target.position, controlPos, true));
        return _movingCo;
    }

    public Coroutine PlayWholeConfiguredPath()
    {
        int pointCount = GetPointCount();
        if (pointCount < 2)
        {
            Debug.LogWarning("[CameraMover] Configured path needs at least 2 points.");
            return null;
        }

        return PlayConfiguredPath(0, pointCount - 1);
    }

    public Coroutine PlayConfiguredPath(int startPointIndex, int endPointIndex)
    {
        if (targetCamera == null) return null;

        int pointCount = GetPointCount();
        if (pointCount < 2)
        {
            Debug.LogWarning("[CameraMover] Configured path needs at least 2 points.");
            return null;
        }

        startPointIndex = Mathf.Clamp(startPointIndex, 0, pointCount - 1);
        endPointIndex = Mathf.Clamp(endPointIndex, 0, pointCount - 1);

        if (startPointIndex >= endPointIndex)
        {
            Debug.LogWarning("[CameraMover] endPointIndex must be greater than startPointIndex.");
            return null;
        }

        StopMoving();
        _movingCo = StartCoroutine(Co_PlayConfiguredPath(startPointIndex, endPointIndex));
        return _movingCo;
    }

    public void SnapToPoint(int pointIndex)
    {
        if (targetCamera == null) return;

        Transform point = GetPoint(pointIndex);
        if (point == null)
        {
            Debug.LogWarning($"[CameraMover] Point index {pointIndex} is invalid.");
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        targetCamera.transform.position = new Vector3(point.position.x, point.position.y, camPos.z);
    }

    private IEnumerator Co_PlayConfiguredPath(int startPointIndex, int endPointIndex)
    {
        if (snapToStartPointBeforePlay)
        {
            SnapToPoint(startPointIndex);
        }

        for (int i = startPointIndex; i < endPointIndex; i++)
        {
            Transform nextPoint = GetPoint(i + 1);
            if (nextPoint == null)
            {
                Debug.LogWarning($"[CameraMover] Missing point at index {i + 1}.");
                break;
            }

            Transform controlPoint = GetSegmentControl(i);
            Vector3? controlPos = controlPoint != null ? controlPoint.position : (Vector3?)null;
            bool notifyDoneAtEnd = (i == endPointIndex - 1);

            yield return Co_MoveTo(nextPoint.position, controlPos, notifyDoneAtEnd);
        }

        _movingCo = null;
    }

    private IEnumerator Co_MoveTo(Vector3 targetPos, Vector3? controlPos, bool notifyDoneAtEnd)
    {
        Vector3 start = targetCamera.transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, start.z);

        Vector3? control = null;
        if (controlPos.HasValue)
        {
            control = new Vector3(controlPos.Value.x, controlPos.Value.y, start.z);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float k = ease != null ? ease.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);

            if (control.HasValue)
            {
                targetCamera.transform.position = GetQuadraticBezierPoint(start, control.Value, end, k);
            }
            else
            {
                targetCamera.transform.position = Vector3.LerpUnclamped(start, end, k);
            }

            yield return null;
        }

        targetCamera.transform.position = end;

        if (notifyDoneAtEnd && CookingManager.instance != null)
        {
            CookingManager.instance.CamTransitionDone();
        }
    }

    private Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0
             + 2f * oneMinusT * t * p1
             + t * t * p2;
    }

    private int GetPointCount()
    {
        return pointsRoot != null ? pointsRoot.childCount : 0;
    }

    private Transform GetPoint(int index)
    {
        if (pointsRoot == null) return null;
        if (index < 0 || index >= pointsRoot.childCount) return null;
        return pointsRoot.GetChild(index);
    }

    private Transform GetSegmentControl(int segmentIndex)
    {
        if (segmentsRoot == null) return null;
        if (segmentIndex < 0 || segmentIndex >= segmentsRoot.childCount) return null;

        Transform segment = segmentsRoot.GetChild(segmentIndex);
        if (segment.childCount < 1) return null;

        return segment.GetChild(0);
    }
}
