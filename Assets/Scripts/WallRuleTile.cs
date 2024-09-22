using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Wall Rule Tile", menuName = "Custom Tiles/Wall Rule Tile")]
public class WallRuleTile : RuleTile<WallRuleTile.Neighbor>
{
    public Tile groundTile;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Ground = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) => neighbor switch
    {
        Neighbor.Ground => tile is not null && tile.name.Equals("Ground"),
        _ => base.RuleMatch(neighbor, tile)
    };
}