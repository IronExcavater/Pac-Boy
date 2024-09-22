using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// Otherwise known as a simplified RuleTile, hehe
[CreateAssetMenu(fileName = "Wall Tile", menuName = "Custom Tiles/Wall Tile")]
public class WallTile : Tile
{
    public Sprite[] sprites;
    public Vector2 anchor = Vector2.zero;
    public Vector2[] neighborAnchors = new Vector2[4];

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
        RefreshNeighbors(position, tilemap);
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
}