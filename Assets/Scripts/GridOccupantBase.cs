using UnityEngine;
using DG.Tweening;

public abstract class GridOccupantBase : MonoBehaviour, IGridOccupant
{
    public virtual Vector2Int GridPosition { get;  set; }
    publicÅ@abstract bool ObstructsMovement { get; }

    public virtual void MoveTo(Vector2Int newPos)
    {
        GridManager.Instance.Unregister(this, GridPosition);
        GridPosition = newPos;
        transform.position = GridManager.GridToWorld(GridPosition);
        GridManager.Instance.Register(this, GridPosition);
    }

    public void Move(Vector2Int moveDir)
    {
        MoveTo(GridPosition + moveDir);
    }

    public virtual bool CanMoveTo(Vector2Int newPos)
    {
        return !GridManager.Instance.HasObstacleOccupants(newPos);
    }
}

