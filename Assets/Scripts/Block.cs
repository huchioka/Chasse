using UnityEngine;

/// <summary>
/// Represents a block on the grid. Can be grabbed and moved by a player of the owning team.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Block :  GridOccupantBase
{
    public Team Team;
    public Player GrabbingPlayer { get; private set; }

    public override bool ObstructsMovement => true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Initialize(GridManager.WorldToGrid(transform.position), Team);
    }

    public void Initialize(Vector2Int pos, Team team)
    {
        GridPosition = pos;
        Team = team;
        spriteRenderer.color = team.TeamColor;
        transform.position = GridManager.GridToWorld(GridPosition);
        GridManager.Instance.Register(this, GridPosition);
    }

    public void SetGrabbedBy(Player player)
    {
        GrabbingPlayer = player;
    }

    public void SetTeam(Team newTeam)
    {
        Team = newTeam;
        spriteRenderer.color = newTeam.TeamColor;
    }

    public override string ToString() => $"Block({name}, {GridPosition}, Team: {Team})";
}