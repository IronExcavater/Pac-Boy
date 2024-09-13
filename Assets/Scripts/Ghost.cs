using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : Character
{
    [Header("Attributes:")] 
    [SerializeField] private Type type;
    
    public Vector3Int _targetPos;

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
    }

    protected override void NextPos()
    {
        
        var possiblePos = GetPossiblePositions(_nextPos);
        _currentPos = _nextPos;
        transform.position = _currentPos; // Recenter position, removing slight offset due to MoveTowards()

        try
        {
            _nextPos = possiblePos
                .OrderBy(pos => Vector3Int.Distance(pos, _targetPos))
                .First();
        } catch (InvalidOperationException) {} // Happens if no possible positions (thrown by First()) -> automatically reverses ghost
        
        Facing = ToDirection(_nextPos - _currentPos);
        Moving = !_nextPos.Equals(_currentPos);
    }
    
    private List<Vector3Int> GetPossiblePositions(Vector3Int origin)
    {
        var map = GameManager.LevelTilemap();
        Vector3Int[] positions =
        {
            origin + Vector3Int.up,
            origin + Vector3Int.right,
            origin + Vector3Int.down,
            origin + Vector3Int.left
        };
        return positions
            .Where(pos =>
                pos != _currentPos &&
                map.GetTile(pos) is Tile tile && tile.Equals(GameManager.GroundTile()))
            .ToList();
    }

    public override void ChangeMode(GameManager.Mode mode)
    {
        switch (mode)
        {
            case GameManager.Mode.Chase:
                IsArmed = true;
                break;
            case GameManager.Mode.Scatter:
                break;
            case GameManager.Mode.Scared:
                IsArmed = false;
                break;
        }
    }
}
