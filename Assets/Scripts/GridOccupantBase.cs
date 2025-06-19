using UnityEngine;
using DG.Tweening;

public abstract class GridOccupantBase : MonoBehaviour, IGridOccupant
{
    public virtual Vector2Int GridPos { get;  set; }
    publicÅ@abstract bool ObstructsMovement { get; }

    public virtual void MoveTo(Vector2Int newPos)
    {
        GridManager.Instance.Unregister(this, GridPos);
        GridPos = newPos;
        transform.position = GridManager.GridToWorld(GridPos);
        GridManager.Instance.Register(this, GridPos);
    }

    public void Move(Vector2Int moveDir)
    {
        MoveTo(GridPos + moveDir);
    }

    public virtual bool CanMoveTo(Vector2Int newPos)
    {
        return !GridManager.Instance.HasObstacleOccupants(newPos);
    }
}

