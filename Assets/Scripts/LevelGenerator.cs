using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2Int = UnityEngine.Vector2Int;

public class LevelGenerator : MonoBehaviour
{
    private readonly int[,] _levelArray =
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
        
        var anchorArray = GenerateAnchors(_levelArray);
        anchorArray = GenerateAnchors(_levelArray, anchorArray);
    }

    private void MirrorLevel()
    {
        var lengthY = _levelArray.GetLength(0);
        var lengthX = _levelArray.GetLength(1);
        var newLengthY = lengthY / 2 * 4; // Ensures that odd lengths won't repeat the mid-tile
        var newLengthX = lengthX / 2 * 4;
        var newLevelArray = new int[newLengthY, newLengthX];
        
    }

    private Vector3Int[,] GenerateAnchors(int[,] levelArray, Vector3Int[,] anchorArray = null)
    {
        var lengthY = levelArray.GetLength(0);
        var lengthX = levelArray.GetLength(1);

        var reverse = anchorArray != null;
        anchorArray ??= new Vector3Int[lengthY, lengthX];

        var step = reverse ? -1 : 1;
        var startY = levelArray.GetLength(0) * -step;
        var startX = levelArray.GetLength(1) * -step;
        var offset = new Vector2Int(reverse ? -1 : lengthX, reverse ? -1 : lengthY);
        print("Offset: " + offset);

        for (var y = startY; y != 0; y += step)
        {
            for (var x = startX; x != 0; x += step)
            {
                var arrayPosition = new Vector2Int(x + offset.x, y + offset.y);
                print("arrayPosition: " + arrayPosition);

                var type = levelArray[arrayPosition.y, arrayPosition.x];
                if (type == 0) continue; // Unknown
                
                var worldPosition = mapOrigin + new Vector3Int(arrayPosition.x, -arrayPosition.y);
                
                if (!IsWall(type)) // Item
                {
                    if (anchorArray[arrayPosition.y, arrayPosition.x].z == 3) continue;
                    Instantiate(ItemObject(type), worldPosition, Quaternion.identity, _map.transform);
                    anchorArray[arrayPosition.y, arrayPosition.x] = new Vector3Int(0, 0, 3);
                    continue;
                }
                
                // Tile
                if (arrayPosition.Equals(Vector2Int.zero)) 
                    anchorArray[0, 0] = new Vector3Int(1, -1, 2);
                else
                    anchorArray[arrayPosition.y, arrayPosition.x] = SetAnchor(arrayPosition, type,
                        NeighborTypes(levelArray, arrayPosition), NeighborAnchors(anchorArray, arrayPosition));
                
                var anchor = anchorArray[arrayPosition.y, arrayPosition.x];
                
                _map.SetTile(worldPosition, Tile(anchor, type));
                _map.SetTransformMatrix(worldPosition, SetRotation(anchor));
            }
        }

        return anchorArray;
    }

    private static Vector3Int SetAnchor(Vector2Int arrayPosition, int type, int[] types, Vector3Int[] anchors)
    {
        switch (type)
        {
            case 1 or 3 or 7:
                if (IsTested(arrayPosition)) print("index: " + arrayPosition);
                for (var i = 0; i < anchors.Length; i++)
                {
                    if (Is2DZero(anchors[i])) continue;
                    var iMod2 = i % 2;
                    if (anchors[i][iMod2] == 0) continue;
                    if (anchors[i].z == 2)
                    {
                        if (IsTested(arrayPosition)) print("i: " + i + ", [1 - iMod2]: " + anchors[i][1 - iMod2] + " = " + (i / 2 == 0 ? 1 : -1));
                        if (anchors[i][1 - iMod2] == (i / 2 == 0 ? 1 : -1)) continue;
                    }

                    for (var j = 0; j < anchors.Length; j++)
                    {
                        var jMod2 = j % 2;
                        if (jMod2 == iMod2 || !IsWall(types[j])) continue;
                        if (!Is2DZero(anchors[j]) && anchors[j][jMod2] == 0) continue;
                        if (anchors[j].z == 2)
                        {
                            if (IsTested(arrayPosition)) print("j: " + j + ", [1 - jMod2]: " + anchors[j][1 - jMod2] + " = " + (j / 2 == 0 ? 1 : -1));
                            if (anchors[j][1 - jMod2] == (j / 2 == 0 ? 1 : -1)) continue;
                        }
                        
                        var secondValue = anchors[i][iMod2];
                        if (i / 2 != j / 2)
                            secondValue *= -1;
                        
                        var indexOffset = anchors[i][iMod2] * (i == 1 || i == 2 ? -1 : 1);
                        var cornerType = (i + indexOffset + 4) % 4 == j ? 2 : 1; // 2: Inner corner, 1: Outer corner

                        if (IsTested(arrayPosition))
                        {
                            print("i: " + i + ", j: " + j);
                            print("secondValue: " + secondValue);
                            print("indexOffset: " + indexOffset + ", cornerType: " + cornerType);
                        }
                        
                        return new Vector3Int(
                                   anchors[i][0] * (1 - iMod2), 
                                   anchors[i][1] * iMod2) +
                               new Vector3Int(
                                   secondValue * iMod2,
                                   secondValue * (1 - iMod2),
                                   cornerType);
                    }
                }
                if (anchors.All(Is2DZero)) return new Vector3Int(-1, 1, 1);
                break;
            case 2 or 4:
                for (var i = 0; i < anchors.Length; i++)
                {
                    if (Is2DZero(anchors[i])) continue;
                    var iMod2 = i % 2;
                    if (anchors[i][iMod2] == 0) continue;
                    
                    if (IsTested(arrayPosition)) print("i: " + i + " = " + anchors[i]);
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

    private static int[] NeighborTypes(int[,] levelArray, Vector2Int arrayPosition)
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
            if (ValidPosition(levelArray.GetLength(0), levelArray.GetLength(1), neighbors[i]))
                types[i] = levelArray[neighbors[i].y, neighbors[i].x];
        
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

    private static void PrintAnchorArray(Vector3Int[,] anchorArray)
    {
        var str = "";
        for (var y = 0; y < anchorArray.GetLength(0); y++)
        {
            for (var x = 0; x < anchorArray.GetLength(1); x++)
            {
                if (x != 0) str += ", ";
                str += anchorArray[y, x];
                str += new string(' ', 12 - anchorArray[y, x].ToString().Length);
            }
            str += "\n";
        }
        print(str);
    }

    private static bool IsTested(Vector2Int arrayPosition)
    {
        return arrayPosition.Equals(new Vector2Int(13, 7)) ||
               arrayPosition.Equals(new Vector2Int(8, 9)) ||
               arrayPosition.Equals(new Vector2Int(8, 10)) ||
               arrayPosition.Equals(new Vector2Int(8, 13));
    }
}
