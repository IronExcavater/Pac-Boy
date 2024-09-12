using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] protected Animator ani;

    private readonly Dictionary<string, int> _aniHash = new();
    private Direction _facing;
    private bool _isArmed;

    public enum Direction
    {
        North,
        East,
        South,
        West
    }
    
    public Direction Facing
    {
        get => _facing;
        protected set
        {
            if (_facing == value) return;
            _facing = value;
            ChangeFacing();
        }
    }

    public bool IsArmed
    {
        get => _isArmed;
        protected set
        {
            if (_isArmed == value) return;
            _isArmed = value;
            StartCoroutine(BlinkTransition("Armed", 4));
        }
    }
    
    protected virtual void Start()
    {
        CacheAnimatorHashes();
        GameManager.RegisterCharacter(this);
        Facing = Direction.South;
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
    
    private void ChangeFacing()
    {
        switch (Facing)
        {
            case Direction.North:
                ani.SetFloat(GetAnimatorHash("Direction"), 0);
                break;
            case Direction.East or Direction.West:
                ani.SetFloat(GetAnimatorHash("Direction"), 1);
                FlipCharacter();
                break;
            case Direction.South:
                ani.SetFloat(GetAnimatorHash("Direction"), 2);
                break;
        }
    }

    private void FlipCharacter()
    {
        // Flip the character by inverting the x scale
        var scale = transform.localScale;
        scale.x = Facing == Direction.West ? -1 : 1;
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
