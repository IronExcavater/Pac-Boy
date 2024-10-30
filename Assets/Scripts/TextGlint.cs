using System.Collections;
using TMPro;
using UnityEngine;

public class TextGlint : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro;
    public float glintSpeed = 1f;
    public float glintLength = 4f;
    public Color glintColor = Color.white;
    private Color _originalColor;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _originalColor = _textMeshPro.color;
        StartCoroutine(GlintEffect());
    }

    private IEnumerator GlintEffect()
    {
        string text = _textMeshPro.text;
        
        while (true)
        {
            var modifiedText = "";

            for (var i = 0; i < text.Length; i++)
            {
                var glintValue = Mathf.Clamp01(Mathf.Sin(Time.time * glintSpeed + i / glintLength));
                var color = Color.Lerp(_originalColor, glintColor, glintValue);
                modifiedText += $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text[i]}</color>";
            }

            _textMeshPro.text = modifiedText;
            yield return null;
        }
    }
}
