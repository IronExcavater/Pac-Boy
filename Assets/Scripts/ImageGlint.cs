using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageGlint : MonoBehaviour
{
    private Image _image;
    public float glintSpeed = 1f;
    public float glintLength = 4f;
    public Color glintColor = Color.white;
    private Color _originalColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalColor = _image.color;
        StartCoroutine(GlintEffect());
    }

    private IEnumerator GlintEffect()
    {
        while (true)
        {
            var glintValue = Mathf.Clamp01(Mathf.Sin(Time.time * glintSpeed / glintLength));
            var color = Color.Lerp(_originalColor, glintColor, glintValue);
            _image.color = color;
            yield return null;
        }
    }
}
