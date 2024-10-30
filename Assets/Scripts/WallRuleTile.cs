using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Wall Rule Tile")]
public class WallRuleTile : RuleTile<WallRuleTile.Neighbor>
{
    public Tile groundTile;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Ground = 3;
        public const int NotGround = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) => neighbor switch
    {
        Neighbor.Ground => tile is not null && tile.name.Equals("Ground"),
        Neighbor.NotGround => tile is null,
        _ => base.RuleMatch(neighbor, tile)
    };
}