using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZoneShaderManager : MonoBehaviour
{
    public static ZoneShaderManager Instance { get; private set; }

    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float transitionDuration = 2f;

    private ColorAdjustments _colorAdjustments;
    private Color _currentColor;
    private Color _targetColor = Color.white;
    private float _progress = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        postProcessVolume.profile.TryGet(out _colorAdjustments);
    }

    public void SetTint(Color color)
    {
        _currentColor = _colorAdjustments.colorFilter.value;
        _targetColor = color;
        _progress = 0f;
    }

    public void ClearTint()
    {
        SetTint(Color.white);
    }

    private void Update()
    {
        if (_progress >= 1f) return;

        _progress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime / transitionDuration);
        Color lerped = Color.Lerp(_currentColor, _targetColor, _progress);
        _colorAdjustments.colorFilter.Override(lerped);
    }
}
