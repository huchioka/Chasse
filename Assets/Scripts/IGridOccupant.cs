using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// Interface for anything occupying a grid cell.
/// </summary>
public interface IGridOccupant
{
    Vector2Int GridPos { get; }
    bool ObstructsMovement { get; }
}