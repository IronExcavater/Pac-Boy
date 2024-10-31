using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] protected Animator ani;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private GameObject dustPrefab;
    private ParticleSystem _dustParticle;
    
    private readonly Dictionary<string, int> _aniHash = new();

    [Header("Attributes:")]
    [SerializeField] private string identifier;

    [SerializeField] private Vector3 spawnPosition;
    private Direction _facing;
    private bool _isArmed;
    private bool _isAlive;
    private bool _isLocked;

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    protected static Direction ToDirection(Vector3 vector)
    {
        if (vector.Equals(Vector3.zero))
            return Direction.South;
        
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
    
    public Vector3 CurrentPosition { get; protected set; }
    public Vector3 NextPosition { get; protected set; }

    public bool IsArmed
    {
        get => _isArmed;
        protected set
        {
            _isArmed = value;
            ani.SetFloat(GetAnimatorHash("Armed"), _isArmed ? 1 : 0);
        }
    }

    public bool IsAlive
    {
        get => _isAlive;
        set
        {
            _isAlive = value;
            ani.SetBool(GetAnimatorHash("Alive"), _isAlive);
        }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }

    public void Attack()
    {
        Lock();
        ani.SetTrigger(GetAnimatorHash("Attack"));
        StartCoroutine(Unlock(1));
    }

    public virtual void Death()
    {
        Lock();
        IsAlive = false;
    }

    public bool Moving
    {
        get => ani.GetBool(GetAnimatorHash("Moving"));
        set => ani.SetBool(GetAnimatorHash("Moving"), value);
    }
    
    protected virtual void Awake()
    {
        CacheAnimatorHashes();
        GameManager.RegisterCharacter(identifier, this);
        _dustParticle = Instantiate(dustPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        _dustParticle.Stop();
    }

    public void Spawn()
    {
        AnimationManager.RemoveTween(transform);
        transform.position = spawnPosition;
        CurrentPosition = transform.position;
        NextPosition = CurrentPosition;
        IsAlive = true;
        IsLocked = true;
        Facing = Direction.East;
        UpdateAnimator();
    }

    protected void DustParticle()
    {
        var shape = _dustParticle.shape;
        shape.rotation = new Vector3(0, 0, DustDirection(Facing));
        _dustParticle.transform.position = transform.position;
        _dustParticle.Emit(1);
    }
    
    private static float DustDirection(Direction direction) => direction switch
    {
        Direction.North => 180,
        Direction.East => 90,
        Direction.South => 0,
        Direction.West => 270,
        _ => 0
    };

    private void OnDestroy()
    {
        GameManager.UnregisterCharacter(identifier);
    }

    private void CacheAnimatorHashes()
    {
        foreach (var parameter in ani.parameters)
            _aniHash.Add(parameter.name, parameter.nameHash);
    }

    protected int GetAnimatorHash(string parameterName)
    {
        return _aniHash.TryGetValue(parameterName, out var hash) ? hash : throw new KeyNotFoundException(parameterName);
    }
    
    private void ChangeFacing()
    {
        ani.SetFloat(GetAnimatorHash("Direction"), (int)Facing % 2 == 0 ? (int)Facing : 1);
        if (Facing == Direction.West) rend.flipX = true;
        if (Facing == Direction.East) rend.flipX = false;
    }

    protected IEnumerator BlinkTransition(string parameterName, float transitionSeconds)
    {
        var initial = ani.GetFloat(GetAnimatorHash(parameterName));
        for (var i = 0; i < transitionSeconds * 10; i++)
        {
            ani.SetFloat(GetAnimatorHash(parameterName), i % 2 == 0 ? 1 - initial : initial);
            yield return new WaitForSeconds(0.1f);
        }
        ani.SetFloat(GetAnimatorHash(parameterName), initial);
    }

    protected void UpdateAnimator()
    {
        rend.sortingOrder = -(int)transform.position.y;
        Facing = ToDirection(NextPosition - CurrentPosition);
        Moving = !NextPosition.Equals(CurrentPosition) && !IsLocked;
    }

    public void Lock()
    {
        IsLocked = true;
        AnimationManager.RemoveTween(transform);
        UpdateAnimator();
    }

    public void Unlock()
    {
        IsLocked = false;
        UpdateAnimator();
        AddTween();
    }

    public IEnumerator Unlock(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        Unlock();
    }

    public abstract void TriggerMode();

    protected abstract void AddTween();

    protected abstract void NextPos();
}
