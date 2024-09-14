using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Ghost : Character
{
    [SerializeField] private Type type;
    [SerializeField] private Vector3Int scatterPos;
    
    private Vector3Int _targetPos;

    private enum Type
    {
        Blinky, // 'Chaser', targets player's current tile
        Pinky,  // 'Ambusher', targets four tiles in front of player
        Inky,   // 'Bashful', targets tile that intersects vector of Blinky's tile and two tiles in front of player at a distance from the player equal to Blinky's distance from the player
        Clyde   // 'Ignorant', targets player similar to Blinky until 8 tiles away, in which case he targets his scatter target tile
    }

    protected override void Start()
    {
        base.Start();
        _nextPos = _currentPos;
    }

    private void Update()
    {
        if (Vector3.Distance(_nextPos, transform.position) < 0.01)
        {
            TargetPos();
            NextPos();
        }
        transform.position = Vector3.MoveTowards(transform.position, _nextPos, GameManager.CharacterSpeed() * Time.deltaTime);
    }

    private void TargetPos()
    {
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase:
                switch (type)
                {
                    case Type.Blinky:
                        _targetPos = GameManager.PlayerPosition;
                        break;
                    case Type.Pinky:
                        _targetPos = GameManager.PlayerPosition + ToVector3Int(GameManager.PlayerFacing) * 4;
                        break;
                    case Type.Inky:
                        throw new NotImplementedException();
                        break;
                    case Type.Clyde:
                        throw new NotImplementedException();
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
        var previousPos = _currentPos;
        _currentPos = _nextPos;
        transform.position = _currentPos; // Recenter position, removing slight offset due to MoveTowards()
        var possiblePos = GetPossiblePositions(Vector3Int.RoundToInt(_currentPos), Vector3Int.RoundToInt(previousPos));

        if (GameManager.GameMode == GameManager.Mode.Scared)
        {
            _nextPos = possiblePos[Random.Range(0, possiblePos.Count)];
        }
        else
        {
            try
            {
                _nextPos = possiblePos
                    .OrderBy(pos => Vector3Int.Distance(pos, _targetPos))
                    .First();
            } catch (InvalidOperationException) {} // Happens if no possible positions (thrown by First()) -> automatically reverses ghost   
        }

        UpdateAnimator();
    }

    private void ReversePos()
    {
        var temp = _currentPos;
        _nextPos = _currentPos;
        _currentPos = temp;
        UpdateAnimator();
    }
    
    private List<Vector3Int> GetPossiblePositions(Vector3Int current, Vector3Int previous)
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
                map.GetTile(pos) is Tile tile && tile.Equals(GameManager.GroundTile()))
            .ToList();
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
}
