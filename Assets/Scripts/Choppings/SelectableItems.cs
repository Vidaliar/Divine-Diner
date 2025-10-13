using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableItems : MonoBehaviour
{
    [SerializeField] private bool enableHoverHighlight = true;
    [SerializeField] private float highlightMultiplier = 1.2f;

    [SerializeField, Min(1)] private int totalSlices = 10; // how many the item can be and should be cut

    private SpriteRenderer _sr;
    private Color _baseColor;
    private bool _isHovered;

    public bool IsSelected { get; private set; }
    public int TotalSlices => Mathf.Max(1, totalSlices);

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _baseColor = _sr.color;
    }

    private void OnMouseEnter()
    {
        if (!enableHoverHighlight || _sr == null) return;
        _isHovered = true;
        _sr.color = _baseColor * highlightMultiplier;
    }

    private void OnMouseExit()
    {
        if (!enableHoverHighlight || _sr == null) return;
        _isHovered = false;
        if (!IsSelected) _sr.color = _baseColor;
    }

    public void MarkSelected(bool selected)
    {
        IsSelected = selected;
        if (_sr == null) return;

        if (selected)
            _sr.color = _baseColor * highlightMultiplier;
        else
            _sr.color = _isHovered ? _baseColor * highlightMultiplier : _baseColor;
    }
}

