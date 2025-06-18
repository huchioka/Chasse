using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the sparse, infinite grid and handles registration/lookup of objects at grid positions.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // Maps grid positions to a list of IGridOccupant
    private Dictionary<Vector2Int, HashSet<IGridOccupant>> grid = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple GridManager instances detected. Destroying duplicate.");
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Converts grid position to world position (Z axis is always 0, grid's Y is up).
    /// </summary>
    public static Vector3 GridToWorld(Vector2Int gridPos) => new(gridPos.x, gridPos.y, 0);

    /// <summary>
    /// Converts a world position to grid coordinates (rounded to nearest int).
    /// </summary>
    public static Vector2Int WorldToGrid(Vector3 worldPos) => new(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

    /// <summary>
    /// Registers an IGridOccupant at a grid position.
    /// </summary>
    public void Register(IGridOccupant occupant, Vector2Int pos)
    {
        if (!grid.TryGetValue(pos, out var set))
        {
            set = new();
            grid[pos] = set;
        }
        set.Add(occupant);
        Debug.Log($"[GridManager] Registered {occupant} at {pos}");
        if (set.Count > 1)
        {
            Debug.Log($"[GridManager] overlap occured.");
        }
    }

    /// <summary>
    /// Unregisters an IGridOccupant from a grid position.
    /// </summary>
    public void Unregister(IGridOccupant occupant, Vector2Int pos)
    {
        if (grid.TryGetValue(pos, out var set))
        {
            set.Remove(occupant);
            if (set.Count == 0)
                grid.Remove(pos);
            Debug.Log($"[GridManager] Unregistered {occupant} from {pos}");
        }
    }

    /// <summary>
    /// Returns all occupants at a given grid position.
    /// </summary>
    public HashSet<IGridOccupant> GetOccupants(Vector2Int pos)
    {
        grid.TryGetValue(pos, out var set);
        return set ?? new();
    }

    /// <summary>
    /// Returns if there are any obstacle objects at a given grid position.
    /// </summary>
    public bool HasObstacleOccupants(Vector2Int pos)
    {
        foreach (var occ in GetOccupants(pos))
        {
            if (occ.ObstructsMovement)
            {
                return true;
            }
        }
        return false;
    }



    /// <summary>
    /// Checks if a cell contains any block.
    /// </summary>
    public Block GetBlockAt(Vector2Int pos)
    {
        foreach (var occ in GetOccupants(pos))
        {
            if (occ is Block block)
                return block;
        }
        return null;
    }

    /// <summary>
    /// Checks if a cell contains any player.
    /// </summary>
    public Player GetPlayerAt(Vector2Int pos)
    {
        foreach (var occ in GetOccupants(pos))
        {
            if (occ is Player player)
                return player;
        }
        return null;
    }
}