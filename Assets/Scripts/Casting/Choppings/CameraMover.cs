using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
================================================================================
CameraMover
================================================================================
Purpose
- A lightweight, gameplay-agnostic component that smoothly moves a camera from its
  current position to a target Transform in world space.
- Only X/Y are moved. The camera's Z stays unchanged (uses the camera's current Z).

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

Typical Workflow Example
- Create empty GameObjects as "view points" (e.g., Area1ViewPoint / Area2ViewPoint)
  and place them where you want the camera to move.
- Trigger movement from any controller script:

    [SerializeField] private CameraMover cameraMover;
    [SerializeField] private Transform area2ViewPoint;

    private IEnumerator GoToArea2()
    {
        // Option A: fire-and-forget
        cameraMover.MoveTo(area2ViewPoint);

        // Option B: wait until movement completes
        yield return cameraMover.MoveTo(area2ViewPoint);
    }

API
- Coroutine MoveTo(Transform target)
    * Starts moving the camera to target.position (keeping camera Z).
    * Returns a Coroutine that can be yielded on for sequencing.
    * If a previous move is in progress, it will be stopped and replaced.
================================================================================
*/
public class CameraMover : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [SerializeField, Range(0.1f, 5f)] private float moveDuration = 1.0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

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
        if (targetCamera == null || target == null) return null;

        if (_movingCo != null) StopCoroutine(_movingCo);
        _movingCo = StartCoroutine(Co_MoveTo(target.position));
        return _movingCo;
    }


    private IEnumerator Co_MoveTo(Vector3 targetPos)
    {
        Vector3 start = targetCamera.transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, start.z); 

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float k = ease != null ? ease.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            targetCamera.transform.position = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }
        targetCamera.transform.position = end;
        _movingCo = null;
    }
}
