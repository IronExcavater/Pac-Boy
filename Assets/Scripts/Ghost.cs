using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Ghost : Character
{
    public Type type;
    [SerializeField] private Vector3Int scatterPos;
    
    public Vector3 _targetPos;

    public enum Type
    {
        Blinky, // 'Chaser', targets player's current tile
        Pinky,  // 'Ambusher', targets four tiles in front of player
        Inky,   // 'Bashful', targets tile that intersects vector of Blinky's tile and two tiles in front of player at a distance from the player equal to Blinky's distance from the player
        Clyde,   // 'Ignorant', targets player similar to Blinky until 8 tiles away, in which case he targets his scatter target tile
        Hidey   // 'Scared', always flees player's current tile
    }
    
    protected void Update()
    {
        TargetPos();
        if (!AnimationManager.TargetExists(transform)) NextPos();;
        UpdateAnimator();
    }

    private void TargetPos()
    {
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase:
                var player = GameManager.GetCharacter("Player");
                if (player is null) return;
                
                switch (type)
                {
                    case Type.Blinky:
                        _targetPos = player.CurrentPosition;
                        break;
                    case Type.Pinky:
                        _targetPos = player.CurrentPosition + ToVector3Int(player.Facing) * 4;
                        break;
                    case Type.Inky:
                        var blinky = GameManager.GetCharacter("Blinky");
                        if (blinky is null) return;
                        
                        var blinkyToPlayer = player.CurrentPosition - blinky.CurrentPosition;
                        _targetPos = Vector3Int.RoundToInt(blinky.CurrentPosition + blinkyToPlayer * 2);
                        break;
                    case Type.Clyde:
                        _targetPos = Vector3.Distance(CurrentPosition, player.CurrentPosition) > 8 ? _targetPos = player.CurrentPosition : scatterPos;
                        break;
                }
                break;
            case GameManager.Mode.Scatter:
                _targetPos = scatterPos;
                break;
        }
    }

    protected override void NextPos()
    {
        if (!IsAlive) return;
        if (IsLocked) return;
        var previousPos = CurrentPosition;
        CurrentPosition = NextPosition;
        
        var possiblePos = GetPossiblePositions(Vector3Int.RoundToInt(CurrentPosition), Vector3Int.RoundToInt(previousPos));

        if (GameManager.GameMode == GameManager.Mode.Scared)
        {
            if (possiblePos.Count > 0) NextPosition = possiblePos[Random.Range(0, possiblePos.Count)];
        }
        else
            try
            {
                NextPosition = possiblePos
                    .OrderBy(pos => Vector3.Distance(pos, _targetPos))
                    .First();
            } catch (InvalidOperationException) {} // Happens if no possible positions (thrown by First()) -> automatically reverses ghost   
        
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
        if (!IsAlive) return;
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
    
    public override void Death()
    {
        base.Death();
        GameManager.Game.StartCoroutine(GameManager.AddScore(300));
        AudioManager.PlaySfxOneShot(AudioManager.Audio.ghostDefeat);
        AudioManager.PlayMusicImmediate(AudioManager.Audio.musicGhost);
        StartCoroutine(Respawn(5));
    }

    private IEnumerator Respawn(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        Spawn();
        GameManager.CheckForDeadGhosts();
    }
}
