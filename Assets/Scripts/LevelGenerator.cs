using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Vector2Int = UnityEngine.Vector2Int;

// I'd honestly love to add comments and separate this code into more methods, I just wouldn't stand adding so many
// parameters to everything. I think it would look more messy. I'd aspire to be a never-nester but this just got the
// best of me.
public class LevelGenerator : MonoBehaviour
{
    private int[,] _typeArray =
    {
        {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7},
        {2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4},
        {2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 4},
        {2, 6, 4, 0, 0, 4, 5, 4, 0, 0, 0, 4, 5, 4},
        {2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 3},
        {2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
        {2, 5, 3, 4, 4, 3, 5, 3, 3, 5, 3, 4, 4, 4},
        {2, 5, 3, 4, 4, 3, 5, 4, 4, 5, 3, 4, 4, 3},
        {2, 5, 5, 5, 5, 5, 5, 4, 4, 5, 5, 5, 5, 4},
        {1, 2, 2, 2, 2, 1, 5, 4, 3, 4, 4, 3, 0, 4},
        {0, 0, 0, 0, 0, 2, 5, 4, 3, 4, 4, 3, 0, 3},
        {0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 3, 4, 4, 0},
        {2, 2, 2, 2, 2, 1, 5, 3, 3, 0, 4, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0}
    };
    
    private Vector3Int[,] _anchorArray;
    
    [SerializeField] private Vector3Int mapOrigin;

    [Header("Tiles:")]
    [SerializeField] private TileBase sideWallTile;
    [SerializeField] private TileBase innerCornerTile, outerCornerTile, groundTile;
    [SerializeField] private GameObject coinItem, potionItem;

    private Tilemap _map;
    
    private void Start()
    {
        _map = GameManager.LevelTilemap();
        // Clear existing map tiles and items
        _map.ClearAllTiles();
        GameManager.ClearItems();

        MirrorLevel();
        StartCoroutine(GenerateLevel());
    }

    private void MirrorLevel()
    {
        var lengthY = _typeArray.GetLength(0);
        var lengthX = _typeArray.GetLength(1);
        var evenLengthY = lengthY / 2 * 2; // Ensures that odd lengths won't repeat the mid-tile
        var evenLengthX = lengthX / 2 * 2;
        var newLengthY = lengthY + evenLengthY;
        var newLengthX = lengthX + evenLengthX;
        var newTypeArray = new int[newLengthY, newLengthX];
        
        // Copy original data
        for (var y = 0; y < lengthY; y++)
            for (var x = 0; x < lengthX; x++)
                newTypeArray[y, x] = _typeArray[y, x];
        
        // Mirror on x-axis
        for (var y = 0; y < evenLengthY; y++)
            for (var x = 0; x < evenLengthX; x++)
                newTypeArray[newLengthY - 1 - y, x] = newTypeArray[y, x];
        
        // Mirror on y-axis
        for (var y = 0; y < newLengthY; y++)
            for (var x = 0; x < evenLengthX; x++) 
                newTypeArray[y, newLengthX - 1 - x] = newTypeArray[y, x];

        _typeArray = newTypeArray;
    }

    private void AdjustCamera()
    {
        _map.CompressBounds();

        var cam = Camera.main;
        if (cam is null) return;
        var mapCenter = _map.cellBounds.center + _map.transform.position;
        cam.transform.position = new Vector3(mapCenter.x, mapCenter.y, -10);
        cam.orthographicSize = _map.size.y / 2f;
    }

    private IEnumerator GenerateLevel()
    {
        _anchorArray = new Vector3Int[_typeArray.GetLength(0), _typeArray.GetLength(1)];
        yield return StartCoroutine(GenerateWalls(_typeArray, _anchorArray));
        yield return StartCoroutine(GenerateWalls(_typeArray, _anchorArray, true));
        yield return StartCoroutine(GenerateGround(_typeArray, _anchorArray));
        AdjustCamera();
    }

    private IEnumerator GenerateGround(int[,] typeArray, Vector3Int[,] anchorArray)
    {
        for (var y = 0; y < typeArray.GetLength(0); y++)
        {
            for (var x = 0; x < typeArray.GetLength(1); x++)
            {
                if (!IsWall(typeArray[y, x])) continue;
                
                var arrayPosition = new Vector2Int(x, y);
                var anchor = anchorArray[y, x] * new Vector3Int(1, -1, 1);

                var groundPositions = anchor.z switch
                {
                    0 or 2 => new[] { arrayPosition + (Vector2Int)anchor },
                    1 => new[]
                    {
                        arrayPosition + (Vector2Int)anchor,
                        arrayPosition + new Vector2Int(0, anchor.y),
                        arrayPosition + new Vector2Int(anchor.x, 0),
                    },
                    _ => Array.Empty<Vector2Int>()
                };

                foreach (var position in groundPositions)
                {
                    var worldPosition = (Vector3Int)position * new Vector3Int(1, -1, 1) + mapOrigin;
                    if (_map.GetTile(worldPosition) is not null) continue;
                    
                    _map.SetTile(worldPosition, groundTile);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    private IEnumerator GenerateWalls(int[,] typeArray, Vector3Int[,] anchorArray, bool reverse = false)
    {
        var lengthY = typeArray.GetLength(0);
        var lengthX = typeArray.GetLength(1);

        var step = reverse ? -1 : 1;
        var startY = typeArray.GetLength(0) * -step;
        var startX = typeArray.GetLength(1) * -step;
        var offset = new Vector2Int(reverse ? -1 : lengthX, reverse ? -1 : lengthY);
        
        for (var y = startY; y != 0; y += step)
        {
            for (var x = startX; x != 0; x += step)
            {
                var arrayPosition = new Vector2Int(x + offset.x, y + offset.y);

                if (anchorArray[arrayPosition.y, arrayPosition.x] != Vector3Int.zero) continue;
                
                var type = typeArray[arrayPosition.y, arrayPosition.x];
                if (type == 0) continue; // Unknown
                
                var worldPosition = mapOrigin + new Vector3Int(arrayPosition.x, -arrayPosition.y);
                
                if (!IsWall(type)) // Item
                {
                    if (anchorArray[arrayPosition.y, arrayPosition.x].z == 3) continue;
                    Instantiate(ItemObject(type), worldPosition, Quaternion.identity, _map.transform);
                    anchorArray[arrayPosition.y, arrayPosition.x] = new Vector3Int(0, 0, 3);
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                
                // Tile
                if (arrayPosition.Equals(Vector2Int.zero)) 
                    anchorArray[0, 0] = new Vector3Int(1, -1, 2);
                else
                    anchorArray[arrayPosition.y, arrayPosition.x] = SetAnchor(arrayPosition, anchorArray, type,
                        NeighborTypes(typeArray, arrayPosition), NeighborAnchors(anchorArray, arrayPosition), reverse);
                
                var anchor = anchorArray[arrayPosition.y, arrayPosition.x];
                
                if (anchor.Equals(Vector3Int.zero)) continue;
                _map.SetTile(worldPosition, Tile(anchor, type));
                _map.SetTransformMatrix(worldPosition, SetRotation(anchor));
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private static Vector3Int SetAnchor(Vector2Int arrayPosition, Vector3Int[,] anchorArray, int type, int[] types, Vector3Int[] anchors, bool isClockwise = false)
    {
        switch (type)
        {
            case 1 or 3 or 7:
                var neighborStart = isClockwise ? 0 : anchors.Length - 1;
                var neighborStep = isClockwise ? 1 : -1;
                for (var i = neighborStart; isClockwise ? i < anchors.Length : i >= 0; i += neighborStep)
                {
                    if (Is2DZero(anchors[i])) continue;
                    var iMod2 = i % 2;
                    if (anchors[i][iMod2] == 0) continue;
                    if (anchors[i].z == 2 && anchors[i][1 - iMod2] == (i / 2 == 0 ? 1 : -1)) continue;
                    
                    for (var j = neighborStart; isClockwise ? j < anchors.Length : j >= 0; j += neighborStep)
                    {
                        var jMod2 = j % 2;
                        if (jMod2 == iMod2 || !IsWall(types[j])) continue;
                        if (!Is2DZero(anchors[j]) && anchors[j][jMod2] == 0) continue;
                        if (anchors[j].z == 2 && anchors[j][1 - jMod2] == (j / 2 == 0 ? 1 : -1)) continue;
                        
                        if (Is2DZero(anchors[j]) && types.All(IsWall))
                            for (var k = j + neighborStep; isClockwise ? k < anchorArray.Length : k >= 0; k += neighborStep) 
                                if (Is2DZero(anchors[k]))
                                    return Vector3Int.zero;
                        
                        var secondValue = anchors[i][iMod2];
                        if (i / 2 != j / 2)
                            secondValue *= -1;
                        
                        var indexOffset = anchors[i][iMod2] * (i == 1 || i == 2 ? -1 : 1);
                        var cornerType = (i + indexOffset + 4) % 4 == j ? 2 : 1; // 2: Inner corner, 1: Outer corner
                        
                        return new Vector3Int(
                                   anchors[i][0] * (1 - iMod2), 
                                   anchors[i][1] * iMod2) +
                               new Vector3Int(
                                   secondValue * iMod2,
                                   secondValue * (1 - iMod2),
                                   cornerType);
                    }
                }

                if (anchors.All(Is2DZero) && !IsBorder(arrayPosition, anchorArray))
                {
                    // Assumptions
                    if (IsWall(types[0]) && IsWall(types[1])) return new Vector3Int(-1, -1, 1);
                    if (IsWall(types[1]) && IsWall(types[2])) return new Vector3Int(-1, 1, 1);
                    if (IsWall(types[2]) && IsWall(types[3])) return new Vector3Int(1, 1, 1);
                    if (IsWall(types[3]) && IsWall(types[0])) return new Vector3Int(1, -1, 1);
                }
                break;
            case 2 or 4:
                for (var i = 0; i < anchors.Length; i++)
                {
                    if (Is2DZero(anchors[i])) continue;
                    var iMod2 = i % 2;
                    if (anchors[i][iMod2] == 0) continue;
                    
                    return new Vector3Int(anchors[i].x * (1 - iMod2), anchors[i].y * iMod2);
                }
                break;
        }
        return Vector3Int.zero;
    }

    private Object ItemObject(int type) => type switch
    {
        5 => coinItem,
        6 => potionItem,
        _ => null
    };

    private TileBase Tile(Vector3Int anchor, int type) => anchor.z switch
    {
        0 => IsWall(type) ? sideWallTile : null,
        1 => outerCornerTile,
        2 => innerCornerTile,
        _ => null
    };

    private static Matrix4x4 SetRotation(Vector3Int anchor)
    {
        var rotationZ = Mathf.Atan2(anchor.y, anchor.x) * Mathf.Rad2Deg + 360; // Raw angle + 360 (ensures pos+ value)
        rotationZ = (int)(rotationZ / 90) * 90; // Round to closest 90deg (for corners)
        if (anchor.z == 1) rotationZ += 180; // Rotations for outer corners is shifted by 180deg
        rotationZ %= 360; // Restrict domain to 0 < angle < 360
        
        return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationZ), Vector3.one);
    }

    private static int[] NeighborTypes(int[,] typeArray, Vector2Int arrayPosition)
    {
        Vector2Int[] neighbors =
        {
            arrayPosition - Vector2Int.up,
            arrayPosition + Vector2Int.right,
            arrayPosition - Vector2Int.down,
            arrayPosition + Vector2Int.left
        };
        
        var types = new int[4];
        
        for (var i = 0; i < neighbors.Length; i++)
            if (ValidPosition(typeArray.GetLength(0), typeArray.GetLength(1), neighbors[i]))
                types[i] = typeArray[neighbors[i].y, neighbors[i].x];
        
        return types;
    }

    private static Vector3Int[] NeighborAnchors(Vector3Int[,] anchorArray, Vector2Int arrayPosition)
    {
        Vector2Int[] neighbors =
        {
            arrayPosition - Vector2Int.up,
            arrayPosition + Vector2Int.right,
            arrayPosition - Vector2Int.down,
            arrayPosition + Vector2Int.left
        };

        var anchors = new Vector3Int[4];
        
        for (var i = 0; i < neighbors.Length; i++)
            if (ValidPosition(anchorArray.GetLength(0), anchorArray.GetLength(1), neighbors[i]))
                anchors[i] = anchorArray[neighbors[i].y, neighbors[i].x];
        
        return anchors;
    }

    private static bool Is2DZero(Vector3Int anchorValue)
    {
        return new Vector2Int(anchorValue.x, anchorValue.y).Equals(Vector2Int.zero);
    }

    private static bool ValidPosition(int lengthY, int lengthX, Vector2Int position)
    {
        return !(position.y < 0 || position.y >= lengthY || 
                 position.x < 0 || position.x >= lengthX);
    }

    private static bool IsWall(int type)
    {
        return type != 0 && type != 5 && type != 6;
    }

    private static bool IsBorder(Vector2Int arrayPosition, Vector3Int[,] anchorArray)
    {
        return arrayPosition.x % anchorArray.GetLength(1) == 0 || arrayPosition.y % anchorArray.GetLength(0) == 0;
    }
}