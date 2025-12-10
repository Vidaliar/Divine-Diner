using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.LogError("[CameraController2D] 未找到 Camera，请在 Inspector 指定或确保场景存在 Main Camera。");
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
        Vector3 end = new Vector3(targetPos.x, targetPos.y, start.z); // 保持Z

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
