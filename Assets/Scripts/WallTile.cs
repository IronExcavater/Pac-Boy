using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;

// Otherwise known as a simplified RuleTile, hehe
[CreateAssetMenu(fileName = "Wall Tile", menuName = "Custom Tiles/Wall Tile")]
public class WallTile : TileBase
{
    public Sprite[] sprites;
    public Vector2 anchor = Vector2.zero;
    public Type type;
    public Vector2[] neighborAnchors = new Vector2[4];

    public enum Type
    {
        Side,
        Corner
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        RefreshNeighbors(position, tilemap);
        SetRotation(position, tilemap.GetComponent<Tilemap>());
        base.RefreshTile(position, tilemap);
    }

    private void OnValidate()
    {
        if (neighborAnchors.Length != 4) Array.Resize(ref neighborAnchors, 4);
    }

    private void RefreshNeighbors(Vector3Int position, ITilemap tilemap)
    {
        Vector3Int[] positions =
        {
            position + Vector3Int.up,
            position + Vector3Int.right,
            position + Vector3Int.down,
            position + Vector3Int.left
        };

        for (var i = 0; i < positions.Length; i++)
        {
            var neighbor = tilemap.GetTile(positions[i]);
            if (neighbor is WallTile wallTile) neighborAnchors[i] = wallTile.anchor;
            else neighborAnchors[i] = Vector2.positiveInfinity;
        }
    }

    private void SetSprite() {}

    private void SetRotation(Vector3Int position, Tilemap tilemap)
    {
        anchor = Vector2.zero;
        var validAnchors = neighborAnchors
            .Where(nAnchor => !nAnchor.Equals(Vector2.zero) && !nAnchor.Equals(Vector2.positiveInfinity))
            .ToList();
        
        foreach (var nAnchor in validAnchors)
        {
            // Debug.Log(nAnchor);
        }
    
        if (validAnchors.Count != 0) return;
        //sprite = sprites[2];
        anchor = new Vector2(1, -1);
        var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180), Vector3.one);
        tilemap.SetTransformMatrix(position, matrix);
    }
}