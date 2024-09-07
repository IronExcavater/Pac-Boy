using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] protected Animator ani;

    private Dictionary<string, int> _aniHash = new();
    private bool _isFacingRight = true;
    private bool _isArmed;
    
    public bool IsFacingRight
    {
        get => _isFacingRight;
        set
        {
            if (_isFacingRight == value) return;
            _isFacingRight = value;
            FlipCharacter();
        }
    }

    public bool IsArmed
    {
        get => _isArmed;
        set
        {
            if (_isArmed == value) return;
            _isArmed = value;
            StartCoroutine(BlinkTransition("Armed", 4));
        }
    }
    
    private void Awake()
    {
        GameManager.RegisterCharacter(this);
        CacheAnimatorHashes();
    }

    private void OnDestroy()
    {
        GameManager.UnregisterCharacter(this);
    }

    private void CacheAnimatorHashes()
    {
        foreach (var parameter in ani.parameters) _aniHash.Add(parameter.name, parameter.nameHash);
    }

    protected int GetAnimatorHash(string parameterName)
    {
        return _aniHash.TryGetValue(parameterName, out var hash) ? hash : throw new KeyNotFoundException();
    }
    
    private void FlipCharacter()
    {
        // Flip the character by inverting the x scale
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private IEnumerator BlinkTransition(string parameterName, int repeat)
    {
        var initial = ani.GetFloat(GetAnimatorHash(parameterName));
        for (var i = 0; i < repeat * 2 - 1; i++)
        {
            ani.SetFloat(GetAnimatorHash(parameterName), i % 2 == 0 ? 1 - initial : initial);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public abstract void ChangeMode(GameManager.Mode mode);
}
