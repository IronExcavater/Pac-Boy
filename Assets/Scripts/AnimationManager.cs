using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Animation { get; private set; }
    
    private Dictionary<Transform, Tween> _tweens = new();

    public enum Easing
    {
        Linear,
        EaseInCubic,
    }
    
    public class Tween
    {
        public Transform Target { get; private set; }
        public Vector3 StartPos { get; private set; }
        public Vector3 EndPos { get; private set; }
        public float StartTime { get; private set; }
        public float Duration { get; private set; }
        public Easing Type { get; private set; }

        public Tween(Transform target, Vector3 startPos, Vector3 endPos, float startTime, float duration, Easing type)
        {
            Target = target;
            StartPos = startPos;
            EndPos = endPos;
            StartTime = startTime;
            Duration = duration;
            Type = type;
        }
    }
    
    private void Awake()
    {
        if (Animation == null)
        {
            Animation = this;
            DontDestroyOnLoad(Animation);
        }
        else Destroy(this);
    }
    
    public static Tween AddTween(Transform target, Vector3 endPos, float duration, Easing type)
    {
        if (TargetExists(target)) return null;
        Vector3 startPos = target is RectTransform rectTransform
            ? rectTransform.anchoredPosition
            : target.transform.position;
        var tween = new Tween(target, startPos, endPos, Time.time, duration, type);
        Animation._tweens.Add(target, tween);
        return tween;
    }
    
    public static bool TargetExists(Transform target)
    {
        return Animation._tweens.ContainsKey(target);
    }

    public static bool TweenExists(Tween tween)
    {
        return Animation._tweens.ContainsValue(tween);
    }

    public void Update()
    {
        List<Transform> toRemove = new();
        foreach (var tween in _tweens.Values)
        {
            var percent = EasingPercentage(tween);
            var position = Vector3.Lerp(tween.StartPos, tween.EndPos, percent);
            SetPosition(tween.Target, position);
            
            if (Vector3.Distance(position, tween.EndPos) > 0.1f) continue;
            SetPosition(tween.Target, tween.EndPos);
            toRemove.Add(tween.Target);
        }

        foreach (var target in toRemove) _tweens.Remove(target);
    }

    private static void SetPosition(Transform target, Vector3 position)
    {
        if (target is RectTransform rectTransform) 
            rectTransform.anchoredPosition = position;
        else
            target.position = position;
    }

    private static float EasingPercentage(Tween tween) => tween.Type switch
    {
        Easing.EaseInCubic => Mathf.Pow((Time.time - tween.StartTime) / tween.Duration, 3),
        _ => (Time.time - tween.StartTime) / tween.Duration
    };
}
