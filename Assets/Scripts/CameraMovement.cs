using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
================================================================================
CameraMovement.cs ¡ª Smoothly move a camera between points and along waypoint paths
================================================================================

IMPLEMENTATION GUIDE (How to use)

A) Basic single-segment moves
   1) Add this script to any GameObject (e.g., "CameraRig").
   2) Inspector:
      - Target Camera: leave empty to auto-use Main Camera.
      - Movement: set Move Duration, Ease, Use Unscaled Time, Keep Camera Z.
      - (Optional) Orthographic Size Tween: toggle and set target size/duration.
      - (Optional) Events: onMoveStart / onMoveComplete.
   3) Code:
      // From current camera position ¡ú target Transform
      yield return CameraMovement.MoveTo(pointB);

      // Snap to A, then smooth to B
      CameraMovement.MoveBetween(pointA, pointB, snapToFrom: true);

B) Multi-point path (waypoints) ¡ª two ways to provide points

   B1) Use preset waypoints from the Inspector:
       - In the "Path" section, assign "Preset Waypoints" with an ordered list
         of Transforms (A -> B -> C -> ...).
       - Choose how time is computed:
         * Path Uses Total Duration: if checked, the whole path uses "Path Total
           Duration" and each segment is time-weighted by its distance.
         * Otherwise, each segment uses the same "Move Duration".
       - Call in code:
         yield return CameraMovement.MoveThroughPresetPath(
             snapToFirst: true,            // instantly place camera at the first point
             useTotalDurationOverride: null, // null => use the Inspector toggle
             totalDurationOverride: null     // null => use "Path Total Duration"
         );

   B2) Provide waypoints from code (Transform[] or Vector3[]):
      // Using Transforms
      var points = new Transform[] { A, B, C, D };
      yield return CameraMovement.MoveThrough(points,
          snapToFirst: true,
          useTotalDuration: true,
          totalDuration: 2.5f);

      // Using world positions
      var pts = new Vector3[] { p0, p1, p2 };
      yield return CameraMovement.MoveThrough(pts,
          snapToFirst: false,
          useTotalDuration: false,  // each segment takes Move Duration
          totalDuration: 0f);       // ignored when useTotalDuration == false

NOTES
- Works in both 2D and 3D. When "Keep Camera Z" is enabled, only X/Y change.
- Orthographic size tween (if enabled) runs across the entire move:
  * Single-segment: over "Move Duration".
  * Multi-segment path: over the path's total duration.
- Calling any Move* API cancels an ongoing movement.
- All Move* APIs return a Coroutine so you can "yield return" to sequence actions.
================================================================================
*/

[DisallowMultipleComponent]
public class CameraMovement : MonoBehaviour
{
    [Header("Target Camera")]
    [SerializeField] private Camera targetCamera;              // Auto-assigns Main Camera if empty

    [Header("Movement")]
    [SerializeField, Range(0.05f, 10f)] private float moveDuration = 1.0f;   // duration per segment (when not using total path duration)
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;     // Ignore Time.timeScale when true
    [SerializeField] private bool keepCameraZ = true;          // Keep camera's Z unchanged (useful for 2D)

    [Header("Optional: Orthographic Size Tween")]
    [SerializeField] private bool tweenOrthoSize = false;      // Tween orthographic size
    [SerializeField] private float targetOrthoSize = 5f;       // Target size
    [SerializeField, Range(0.05f, 10f)] private float sizeTweenDuration = 1.0f;

    [Header("Path (Waypoints)")]
    [SerializeField] private Transform[] presetWaypoints;      // Assign in Inspector to drive a preset path
    [SerializeField] private bool pathUsesTotalDuration = true;// If true, use Path Total Duration; else each segment uses Move Duration
    [SerializeField, Range(0.05f, 120f)] private float pathTotalDuration = 2.0f; // Total time for entire path when using total duration

    [Header("Events")]
    public UnityEvent onMoveStart;
    public UnityEvent onMoveComplete;

    /// True while a camera movement is in progress.</summary>
    public bool IsMoving { get; private set; }

    private Coroutine _running;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void OnValidate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (ease == null) ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        moveDuration = Mathf.Max(0.05f, moveDuration);
        sizeTweenDuration = Mathf.Max(0.05f, sizeTweenDuration);
        pathTotalDuration = Mathf.Max(0.05f, pathTotalDuration);
    }

    // ============================== PUBLIC API ==============================

    /// Smoothly moves the camera from a starting Transform to a target Transform.
    /// Optionally snaps the camera to the start point before beginning the tween.
    public Coroutine MoveBetween(Transform from, Transform to, bool snapToFrom = true)
    {
        if (from == null || to == null || targetCamera == null) return null;
        return MoveBetween(from.position, to.position, snapToFrom);
    }

    /// Smoothly moves the camera from a starting position to a target position.
    /// Optionally snaps the camera to the start position before beginning the tween.
    public Coroutine MoveBetween(Vector3 from, Vector3 to, bool snapToFrom = true)
    {
        if (targetCamera == null) return null;

        if (snapToFrom)
        {
            Vector3 curr = targetCamera.transform.position;
            targetCamera.transform.position = new Vector3(from.x, from.y, keepCameraZ ? curr.z : from.z);
        }
        return StartMoveToPoint(to);
    }

    /// Smoothly moves the camera from its current position to the target Transform.
    public Coroutine MoveTo(Transform to)
    {
        if (to == null) return null;
        return StartMoveToPoint(to.position);
    }

    /// Smoothly moves the camera from its current position to the target world position.
    public Coroutine MoveTo(Vector3 to)
    {
        return StartMoveToPoint(to);
    }

    /// Smoothly moves the camera along a sequence of waypoint Transforms.
   
    /// <param name="waypoints">Ordered list of Transforms (length >= 2).</param>
    /// <param name="snapToFirst">Instantly place the camera at the first point before moving.</param>
    /// <param name="useTotalDuration">If true, the entire path uses 'totalDuration' and each segment is time-weighted by distance. If false, each segment uses 'moveDuration'.</param>
    /// <param name="totalDuration">Total duration for the whole path when useTotalDuration is true.</param>
    public Coroutine MoveThrough(IList<Transform> waypoints, bool snapToFirst = true, bool useTotalDuration = true, float totalDuration = 2f)
    {
        if (waypoints == null || waypoints.Count < 2) return null;
        var pts = new List<Vector3>(waypoints.Count);
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null) pts.Add(waypoints[i].position);
        }
        if (pts.Count < 2) return null;
        return StartMovePath(pts, snapToFirst, useTotalDuration, totalDuration);
    }

    /// Smoothly moves the camera along a sequence of world positions.
    public Coroutine MoveThrough(IList<Vector3> waypoints, bool snapToFirst = true, bool useTotalDuration = true, float totalDuration = 2f)
    {
        if (waypoints == null || waypoints.Count < 2) return null;
        return StartMovePath(new List<Vector3>(waypoints), snapToFirst, useTotalDuration, totalDuration);
    }
    /// Uses the Inspector-assigned 'Preset Waypoints' to move along a path.
    /// <param name="snapToFirst">Instantly place the camera at the first waypoint before moving.</param>
    /// <param name="useTotalDurationOverride">If null, use the Inspector toggle 'pathUsesTotalDuration'.</param>
    /// <param name="totalDurationOverride">If null, use 'pathTotalDuration'.</param>
    public Coroutine MoveThroughPresetPath(bool snapToFirst = true, bool? useTotalDurationOverride = null, float? totalDurationOverride = null)
    {
        if (presetWaypoints == null || presetWaypoints.Length < 2) return null;
        bool useTotal = useTotalDurationOverride ?? pathUsesTotalDuration;
        float tot = totalDurationOverride ?? pathTotalDuration;
        return MoveThrough((IList<Transform>)presetWaypoints, snapToFirst, useTotal, tot);
    }

    /// Immediately stops the current camera movement (if any).
    public void StopCurrent()
    {
        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
            IsMoving = false;
        }
    }

    // ============================== INTERNAL ===============================

    private Coroutine StartMoveToPoint(Vector3 to)
    {
        StopCurrent();
        _running = StartCoroutine(Co_MoveToPoint(to));
        return _running;
    }

    private Coroutine StartMovePath(List<Vector3> pts, bool snapToFirst, bool useTotalDuration, float totalDuration)
    {
        StopCurrent();
        _running = StartCoroutine(Co_MovePath(pts, snapToFirst, useTotalDuration, totalDuration));
        return _running;
    }

    private IEnumerator Co_MoveToPoint(Vector3 to)
    {
        if (targetCamera == null) yield break;

        onMoveStart?.Invoke();
        IsMoving = true;

        Vector3 pos0 = targetCamera.transform.position;
        Vector3 pos1 = new Vector3(to.x, to.y, keepCameraZ ? pos0.z : to.z);

        bool sizeActive = tweenOrthoSize && targetCamera.orthographic;
        float size0 = sizeActive ? targetCamera.orthographicSize : 0f;
        float size1 = sizeActive ? targetOrthoSize : size0;

        float tPos = 0f;
        float tSize = sizeActive ? 0f : 1f;

        while (tPos < 1f || tSize < 1f)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (tPos < 1f)
            {
                tPos = Mathf.Min(1f, tPos + dt / Mathf.Max(0.0001f, moveDuration));
                float kPos = ease.Evaluate(tPos);
                targetCamera.transform.position = Vector3.LerpUnclamped(pos0, pos1, kPos);
            }

            if (sizeActive && tSize < 1f)
            {
                tSize = Mathf.Min(1f, tSize + dt / Mathf.Max(0.0001f, sizeTweenDuration));
                float kSize = ease.Evaluate(tSize);
                targetCamera.orthographicSize = Mathf.LerpUnclamped(size0, size1, kSize);
            }

            yield return null;
        }

        targetCamera.transform.position = pos1;
        if (sizeActive) targetCamera.orthographicSize = size1;

        IsMoving = false;
        onMoveComplete?.Invoke();
        _running = null;
    }

    private IEnumerator Co_MovePath(List<Vector3> pts, bool snapToFirst, bool useTotalDuration, float totalDuration)
    {
        if (targetCamera == null) yield break;
        if (pts == null || pts.Count < 2) yield break;

        onMoveStart?.Invoke();
        IsMoving = true;

        // Optionally snap to the first waypoint
        Vector3 camPos = targetCamera.transform.position;
        Vector3 first = pts[0];
        if (snapToFirst)
        {
            targetCamera.transform.position = new Vector3(first.x, first.y, keepCameraZ ? camPos.z : first.z);
            camPos = targetCamera.transform.position;
        }

        // Prepare per-segment durations
        int segCount = pts.Count - 1;
        float[] segDur = new float[segCount];
        float totalPathTime;

        if (useTotalDuration)
        {
            // Weight each segment by distance so the camera moves at a constant apparent speed
            float[] segDist = new float[segCount];
            float distSum = 0f;
            for (int i = 0; i < segCount; i++)
            {
                var a = pts[i];
                var b = pts[i + 1];
                float d = Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y)); // XY distance; ignore Z for 2D feel
                segDist[i] = Mathf.Max(0f, d);
                distSum += segDist[i];
            }
            totalPathTime = Mathf.Max(0.05f, totalDuration);
            if (distSum <= 0f)
            {
                // All points coincide; treat as one zero-length segment ¡ú jump immediately
                for (int i = 0; i < segCount; i++) segDur[i] = totalPathTime / segCount;
            }
            else
            {
                for (int i = 0; i < segCount; i++)
                {
                    segDur[i] = totalPathTime * (segDist[i] / distSum);
                    segDur[i] = Mathf.Max(0.0001f, segDur[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < segCount; i++) segDur[i] = Mathf.Max(0.05f, moveDuration);
            totalPathTime = segDur[0] * segCount;
        }

        // Size tween across the whole path duration (if enabled)
        bool sizeActive = tweenOrthoSize && targetCamera.orthographic;
        float size0 = sizeActive ? targetCamera.orthographicSize : 0f;
        float size1 = sizeActive ? targetOrthoSize : size0;
        float elapsedTotal = 0f;

        // Move segment by segment
        for (int i = 0; i < segCount; i++)
        {
            Vector3 a = targetCamera.transform.position; // start from current camera pos
            Vector3 b = pts[i + 1];
            Vector3 pos1 = new Vector3(b.x, b.y, keepCameraZ ? a.z : b.z);

            float t = 0f;
            float dur = segDur[i];

            while (t < 1f)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t = Mathf.Min(1f, t + dt / Mathf.Max(0.0001f, dur));

                float kPos = ease.Evaluate(t);
                targetCamera.transform.position = Vector3.LerpUnclamped(a, pos1, kPos);

                if (sizeActive && elapsedTotal < totalPathTime)
                {
                    elapsedTotal = Mathf.Min(totalPathTime, elapsedTotal + dt);
                    float ks = ease.Evaluate(Mathf.Clamp01(elapsedTotal / totalPathTime));
                    targetCamera.orthographicSize = Mathf.LerpUnclamped(size0, size1, ks);
                }

                yield return null;
            }

            // Snap to end of segment
            targetCamera.transform.position = pos1;
        }

        // Finalize size tween if still pending
        if (sizeActive)
        {
            targetCamera.orthographicSize = size1;
        }

        IsMoving = false;
        onMoveComplete?.Invoke();
        _running = null;
    }
}
