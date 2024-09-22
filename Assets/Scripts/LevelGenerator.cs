using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Layout Map:")]
    private readonly int[,] _levelLayout =
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

    [Header("Attributes:")]
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private Vector3Int mapOrigin;

    private Tilemap _map;
    
    private void Start()
    {
        _map = GameManager.LevelTilemap();
        // Clear existing map tiles
        _map.ClearAllTiles();
        GameManager.ClearItems();
        
        GenerateQuadrant(_levelLayout);
    }

    private void GenerateQuadrant(int[,] levelLayout)
    {
        for (var y = 0; y < levelLayout.GetLength(0); y++)
        {
            for (var x = 0; x < levelLayout.GetLength(1); x++)
            {
                var worldPosition = mapOrigin + new Vector3Int(x, -y);
                var arrayPosition = new Vector2Int(x, y);
                
                switch (levelLayout[arrayPosition.y, arrayPosition.x])
                {
                    case 1 or 3 or 7:
                        _map.SetTile(worldPosition, wallTile);
                        break;
                    case 2 or 4:
                        _map.SetTile(worldPosition, wallTile);
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                }
            }
        }
    }
}
