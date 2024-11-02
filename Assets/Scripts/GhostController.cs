using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GhostController : Character
{
    public Type type;
    public float exitDelay;
    [SerializeField] private Vector3Int scatterPos;
    [SerializeField] private Vector3Int[] followPath;
    private int _followIndex;
    [SerializeField] private GameObject ghostCorpsePrefab;
    [SerializeField] private Sprite stillSprite;
    
    private Vector3 _targetPos;
    private GameObject _ghostCorpse;

    public enum Type
    {
        Blinky, // 'Chaser', targets player's current tile
        Pinky,  // 'Ambusher', targets four tiles in front of player
        Inky,   // 'Bashful', targets tile that intersects vector of Blinky's tile and two tiles in front of player at a distance from the player equal to Blinky's distance from the player
        Clyde,  // 'Ignorant', targets player similar to Blinky until 8 tiles away, in which case he targets his scatter target tile
        Hidey,  // always flees player's current tile
        Clumsy, // moves in random directions
        Bordy   // moves clockwise around the outside wall of the map
    }
    
    protected bool IsSpawn { get; set; }
    
    protected void Update()
    {
        TargetPos();
        if (!AnimationManager.TargetExists(transform)) NextPos();;
        UpdateAnimator();
    }

    private void TargetPos()
    {
        var player = GameManager.GetCharacter("Player");
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase:
                switch (type)
                {
                    case Type.Blinky or Type.Hidey:
                        if (player is null) return;
                        _targetPos = player.CurrentPosition;
                        break;
                    case Type.Pinky:
                        if (player is null) return;
                        _targetPos = player.CurrentPosition + ToVector3Int(player.Facing) * 4;
                        break;
                    case Type.Inky:
                        if (player is null) return;
                        var blinky = GameManager.GetCharacter("Blinky");
                        if (blinky is null) break;
                        
                        var blinkyToPlayer = player.CurrentPosition - blinky.CurrentPosition;
                        _targetPos = Vector3Int.RoundToInt(blinky.CurrentPosition + blinkyToPlayer * 2);
                        break;
                    case Type.Clyde:
                        if (player is null) return;
                        _targetPos = Vector3.Distance(CurrentPosition, player.CurrentPosition) > 8 ? _targetPos = player.CurrentPosition : scatterPos;
                        break;
                    case Type.Bordy:
                        _targetPos = followPath[_followIndex];
                        if (CurrentPosition != followPath[_followIndex]) break;
                        _followIndex = (_followIndex + 1) % followPath.Length;
                        break;
                }
                break;
            case GameManager.Mode.Scatter:
                _targetPos = scatterPos;
                break;
            case GameManager.Mode.Scared:
                if (player is null) return;
                _targetPos = player.CurrentPosition;
                break;
        }
    }

    protected override void NextPos()
    {
        if (!IsAlive || IsLocked || IsSpawn) return;
        var previousPos = CurrentPosition;
        CurrentPosition = NextPosition;
        
        var possiblePos = GetPossiblePositions(Vector3Int.RoundToInt(CurrentPosition), Vector3Int.RoundToInt(previousPos));

        if (GameManager.GameMode == GameManager.Mode.Scared || type == Type.Hidey)
        {
            var filteredPos = possiblePos.FindAll(pos =>
                Vector3.Distance(pos, _targetPos) >= Vector3.Distance(CurrentPosition, _targetPos));
            if (filteredPos.Count > 0) NextPosition = filteredPos[Random.Range(0, filteredPos.Count)];
            else if (possiblePos.Count > 0) NextPosition = possiblePos[Random.Range(0, possiblePos.Count)];
        }
        else if (type == Type.Clumsy)
        {
            if (possiblePos.Count > 0) NextPosition = possiblePos[Random.Range(0, possiblePos.Count)];
        }
        else
        {
            try
            {
                NextPosition = possiblePos
                    .OrderBy(pos => Vector3.Distance(pos, _targetPos))
                    .First();
            }
            catch (InvalidOperationException) {} // Happens if no possible positions (thrown by First()) -> automatically reverses ghost   
        }

        AddTween();
        UpdateAnimator();
        if (!Moving) return;
        DustParticle();
    }
    
    private static List<Vector3Int> GetPossiblePositions(Vector3Int current, Vector3Int previous)
    {
        var map = GameManager.LevelTilemap();
        Vector3Int[] positions =
        {
            current + Vector3Int.up,
            current + Vector3Int.right,
            current + Vector3Int.down,
            current + Vector3Int.left
        };
        return positions
            .Where(pos =>
                pos != previous &&
                map.GetTile(pos) is Tile tile && GameManager.IsGroundTile(tile))
            .ToList();
    }

    private void ReversePos()
    {
        var temp = CurrentPosition;
        NextPosition = CurrentPosition;
        CurrentPosition = temp;
        UpdateAnimator();
        DustParticle();
    }

    public override void TriggerMode()
    {
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase:
                IsArmed = true;
                break;
            case GameManager.Mode.Scatter:
                ReversePos(); // Reverse ghost when scatter mode initiates
                break;
            case GameManager.Mode.Scared:
                IsArmed = false;
                StartCoroutine(RecoveringBlinking(7));
                break;
        }
        UpdateAnimator();
    }

    private IEnumerator RecoveringBlinking(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        yield return StartCoroutine(BlinkTransition("Armed", 3));
        IsArmed = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive || IsSpawn) return;
        var player = GameManager.GetCharacter("Player");
        if (!other.gameObject.Equals(player.gameObject)) return;
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase or GameManager.Mode.Scatter:
                player.Death();
                Attack();
                break;
            case GameManager.Mode.Scared:
                Death();
                player.Attack();
                break;
        }
    }

    protected override void AddTween()
    {
        CurrentPosition = transform.position;
        AnimationManager.AddTween(transform, NextPosition,
            Vector3.Distance(CurrentPosition, NextPosition) / 
            (GameManager.GameMode == GameManager.Mode.Scared ? GameManager.Game.scaredSpeed : GameManager.Game.characterSpeed),
            AnimationManager.Easing.Linear);
    }

    public override void Reset()
    {
        base.Reset();
        IsSpawn = true;
        StartCoroutine(ExitSpawnArea(exitDelay));
    }
    
    public override void Death()
    {
        base.Death();
        GameManager.Game.StartCoroutine(GameManager.AddScore(300));
        AudioManager.PlaySfxOneShot(AudioManager.Audio.ghostDefeat);
        AudioManager.PlayMusicOneShotNextBeat(AudioManager.Audio.musicGhost);
        StartCoroutine(Corpse());
    }

    private IEnumerator Corpse()
    {
        yield return new WaitForSeconds(2);
        _ghostCorpse = Instantiate(ghostCorpsePrefab, transform.position, Quaternion.identity);
        var ghostCorpseRend = _ghostCorpse.GetComponent<SpriteRenderer>();
        ghostCorpseRend.sprite = rend.sprite;
        ghostCorpseRend.sortingOrder = rend.sortingOrder;
        ani.enabled = false;
        rend.sprite = stillSprite;
        rend.color = new Color(0.8f, 0.8f, 1, 0.8f);
        
        NextPosition = spawnPosition;
        AnimationManager.AddTween(transform, NextPosition,
            Vector3.Distance(CurrentPosition, NextPosition) / GameManager.Game.scaredSpeed,
            AnimationManager.Easing.Linear);
        yield return new WaitUntil(() => !AnimationManager.TargetExists(transform));
        CurrentPosition = NextPosition;
        ani.enabled = true;
        IsAlive = true;
        IsSpawn = true;
        GameManager.CheckForDeadGhosts();
        rend.color = Color.white;
        StartCoroutine(FadeOutAndDestroy(_ghostCorpse, 2));
        StartCoroutine(ExitSpawnArea(2));
    }

    private IEnumerator FadeOutAndDestroy(GameObject obj, float duration)
    {
        var objRend = obj.GetComponent<SpriteRenderer>();
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var newAlpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            objRend.color = new Color(1, 1, 1, newAlpha);
            yield return null;
        }
        Destroy(obj);
    }

    private IEnumerator ExitSpawnArea(float delaySeconds)
    {
        yield return new WaitUntil(() => GameManager.GameMode != GameManager.Mode.None);
        yield return new WaitForSeconds(delaySeconds);
        NextPosition = GameManager.LevelTilemap().cellBounds.center + new Vector3(-0.5f, -0.5f);
        AnimationManager.AddTween(transform, NextPosition,
            Vector3.Distance(CurrentPosition, NextPosition) / GameManager.Game.scaredSpeed,
            AnimationManager.Easing.Linear);
        yield return new WaitUntil(() => !AnimationManager.TargetExists(transform));
        CurrentPosition = NextPosition;
        NextPosition = GameManager.LevelTilemap().cellBounds.center + new Vector3(-0.5f, 2.5f);
        AnimationManager.AddTween(transform, NextPosition,
            Vector3.Distance(CurrentPosition, NextPosition) / GameManager.Game.scaredSpeed,
            AnimationManager.Easing.Linear);
        yield return new WaitUntil(() => !AnimationManager.TargetExists(transform));
        CurrentPosition = NextPosition;
        IsSpawn = false;
        Unlock();
    }
}
