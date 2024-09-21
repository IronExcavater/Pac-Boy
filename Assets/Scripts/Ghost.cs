using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ghost : Character
{
    public Type type;
    [SerializeField] private Vector3Int scatterPos;
    
    private Vector3 _targetPos;

    public enum Type
    {
        Blinky, // 'Chaser', targets player's current tile
        Pinky,  // 'Ambusher', targets four tiles in front of player
        Inky,   // 'Bashful', targets tile that intersects vector of Blinky's tile and two tiles in front of player at a distance from the player equal to Blinky's distance from the player
        Clyde   // 'Ignorant', targets player similar to Blinky until 8 tiles away, in which case he targets his scatter target tile
    }

    protected override void Start()
    {
        base.Start();
        NextPosition = CurrentPosition;
    }

    private void Update()
    {
        if (Vector3.Distance(NextPosition, transform.position) < 0.01)
        {
            TargetPos();
            NextPos();
        }
        // TODO: Use bezier curves
        transform.position = Vector3.MoveTowards(transform.position, NextPosition, GameManager.CharacterSpeed() * Time.deltaTime);
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
        var previousPos = CurrentPosition;
        CurrentPosition = NextPosition;
        transform.position = CurrentPosition; // Recenter position, removing slight offset due to MoveTowards()
        
        var possiblePos = GetPossiblePositions(Vector3Int.RoundToInt(CurrentPosition), Vector3Int.RoundToInt(previousPos));

        if (GameManager.GameMode == GameManager.Mode.Scared) 
            NextPosition = possiblePos[Random.Range(0, possiblePos.Count)];
        else
            try
            {
                NextPosition = possiblePos
                    .OrderBy(pos => Vector3.Distance(pos, _targetPos))
                    .First();
            } catch (InvalidOperationException) {} // Happens if no possible positions (thrown by First()) -> automatically reverses ghost   

        UpdateAnimator();
    }

    private void ReversePos()
    {
        var temp = CurrentPosition;
        NextPosition = CurrentPosition;
        CurrentPosition = temp;
        
        UpdateAnimator();
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
                break;
        }
    }
    
    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
