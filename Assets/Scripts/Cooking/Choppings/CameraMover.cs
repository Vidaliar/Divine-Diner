using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Event;

/*
================================================================================
CameraMover
================================================================================
Purpose
- A lightweight, gameplay-agnostic component that smoothly moves a camera from its
  current position to a target Transform in world space.
- Only X/Y are moved. The camera's Z stays unchanged (uses the camera's current Z).
- Supports either straight-line movement or quadratic Bezier curve movement.

How to Use
1) Add this component to any GameObject in the scene (e.g., "CameraRig").
2) In the Inspector:
   - Target Camera:
       * Assign Main Camera
       * or leave it empty to auto-use Camera.main at runtime.
   - Move Duration:
       * Time in seconds to complete the move (clamped to >= 0.1).
   - Ease:
       * AnimationCurve used to smooth the interpolation (default: EaseInOut).
   - Default Control Point (optional):
       * If assigned, MoveTo(target) will move along a quadratic Bezier curve.
       * If left empty, MoveTo(target) stays as straight-line movement.

Typical Workflow Example
- Create empty GameObjects as "view points" (e.g., Area1ViewPoint / Area2ViewPoint)
  and place them where you want the camera to move.
- If you want a curve, add another empty GameObject as the Bezier control point.
- Trigger movement from any controller script:

    [SerializeField] private CameraMover cameraMover;
    [SerializeField] private Transform area2ViewPoint;
    [SerializeField] private Transform area2CurveControl;

    private IEnumerator GoToArea2()
    {
        // Option A: use inspector default control point (if assigned)
        cameraMover.MoveTo(area2ViewPoint);

        // Option B: pass a control point explicitly for this move
        yield return cameraMover.MoveTo(area2ViewPoint, area2CurveControl);
    }

API
- Coroutine MoveTo(Transform target)
    * Starts moving the camera to target.position (keeping camera Z).
    * Uses defaultControlPoint if one is assigned; otherwise moves in a straight line.
- Coroutine MoveTo(Transform target, Transform controlPoint)
    * Starts moving the camera along a quadratic Bezier curve if controlPoint is not null.
    * Falls back to straight-line movement if controlPoint is null.
================================================================================
*/
public class CameraMover : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [SerializeField, Range(0.1f, 5f)] private float moveDuration = 1.0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Transform defaultControlPoint;

    // public UnityEvent doneMoving = new UnityEvent();

    private Coroutine _movingCo;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null)
            Debug.LogError("[CameraController2D] Target camera is missing.");
    }

    private void OnValidate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (moveDuration < 0.1f) moveDuration = 0.1f;
        if (ease == null) ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public Coroutine MoveTo(Transform target)
    {
        return MoveTo(target, defaultControlPoint);
    }

    public Coroutine MoveTo(Transform target, Transform controlPoint)
    {
        if (targetCamera == null || target == null) return null;
        if (_movingCo != null) StopCoroutine(_movingCo);
        Vector3? controlPos = null;
        if (controlPoint != null)
        {
            controlPos = new Vector3(controlPoint.position.x, controlPoint.position.y, targetCamera.transform.position.z);
        }
        _movingCo = StartCoroutine(Co_MoveTo(target.position, controlPos));
        return _movingCo;
    }


    private IEnumerator Co_MoveTo(Vector3 targetPos, Vector3? controlPos)
    {
        Vector3 start = targetCamera.transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, start.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float k = ease != null ? ease.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);

            if (controlPos.HasValue)
            {
                targetCamera.transform.position = GetQuadraticBezierPoint(start, controlPos.Value, end, k);
            }
            else
            {
                targetCamera.transform.position = Vector3.LerpUnclamped(start, end, k);
            }
            yield return null;
        }

        targetCamera.transform.position = end;
        CookingManager.instance.CamTransitionDone();
        _movingCo = null;
    }

    private Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0
             + 2f * oneMinusT * t * p1
             + t * t * p2;
    }
}
