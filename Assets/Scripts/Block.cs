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
        Vector2Int startPos = GridManager.WorldToGrid(transform.position);
        Initialize(startPos, Team);
        Debug.Log($"[Block] a block initialized at {startPos}");
    }

    public void Initialize(Vector2Int pos, Team team)
    {
        GridPos = pos;
        Team = team;
        spriteRenderer.color = team.TeamColor;
        transform.position = GridManager.GridToWorld(GridPos);
        GridManager.Instance.Register(this, GridPos);
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

    public override string ToString() => $"Block({name}, {GridPos}, Team: {Team})";
}