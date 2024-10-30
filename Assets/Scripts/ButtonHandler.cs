using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public RectTransform textTransform;
    private Vector2 _releasePosition;
    private Vector2 _pressPosition;

    private void Awake()
    {
        _releasePosition = textTransform.anchoredPosition;
        _pressPosition = _releasePosition - new Vector2(0, 10);
    }

    public void OnButtonPressed()
    {
        textTransform.anchoredPosition = _pressPosition;
    }
    
    public void OnButtonReleased()
    {
        textTransform.anchoredPosition = _releasePosition;
        AudioManager.PlaySfxOneShot(AudioManager.Audio.select);
    }
}
