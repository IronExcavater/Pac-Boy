using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] protected Animator ani;
    [SerializeField] private SpriteRenderer rend;
    
    private readonly Dictionary<string, int> _aniHash = new();

    [Header("Attributes:")]
    
    private Direction _facing;
    private bool _isArmed;
    protected Vector3 _currentPos;
    protected Vector3 _nextPos;

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    protected static Direction ToDirection(Vector3 vector)
    {
        if (vector.Equals(Vector3.zero)) return Direction.South;
        
        var directionMap = new Dictionary<Direction, Vector3Int>
        {
            { Direction.North, Vector3Int.up },
            { Direction.East, Vector3Int.right },
            { Direction.South, Vector3Int.down },
            { Direction.West, Vector3Int.left },
        };

        return directionMap
            .OrderBy(dir => Vector3.Distance(dir.Value, vector))
            .First().Key;
    }

    protected static Vector3Int ToVector3Int(Direction direction) => direction switch
    {
        Direction.North => Vector3Int.up,
        Direction.East => Vector3Int.right,
        Direction.South => Vector3Int.down,
        Direction.West => Vector3Int.left,
        _ => Vector3Int.zero
    };
    
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

    public bool Moving
    {
        get => ani.GetBool(GetAnimatorHash("Moving"));
        set => ani.SetBool(GetAnimatorHash("Moving"), value);
    }
    
    protected virtual void Start()
    {
        CacheAnimatorHashes();
        GameManager.RegisterCharacter(this);
        UpdateAnimator();
        _currentPos = transform.position;
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

    protected void UpdateAnimator()
    {
        rend.sortingOrder = -(int)transform.position.y;
        Facing = ToDirection(_nextPos - _currentPos);
        Moving = !_nextPos.Equals(_currentPos);
    }

    public abstract void TriggerMode();

    protected abstract void NextPos();
}
