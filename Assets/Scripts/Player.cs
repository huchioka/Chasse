using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls a player piece, grid movement, facing, grabbing blocks, and team membership.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Player : GridOccupantBase
{
    [SerializeField] float MoveCooldown = 0.2f;
    [SerializeField] Team Team;
    public override bool ObstructsMovement => true;
    public SpriteRenderer Renderer { get; private set; }

    private Vector2Int facing = Vector2Int.right;
    private float moveTimer;
    private Vector2Int? queuedMove;
    private List<Block> grabbingBlocks = new(); // Direction → Block

    // Input Actions
    private PlayerInputActions inputActions;

    // Cardinal directions
    static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        Vector2 startPos = new(transform.position.x, transform.position.y);
        Vector2Int roundedStartPos = Vector2Int.RoundToInt(startPos);
        Initialize(roundedStartPos, Team);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMoveInput;
        inputActions.Player.Grab.performed += OnGrabInput;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMoveInput;
        inputActions.Player.Grab.performed -= OnGrabInput;
        inputActions.Disable();
    }

    public void Initialize(Vector2Int startPos, Team team)
    {
        GridPosition = startPos;
        Team = team;
        transform.position = GridManager.GridToWorld(GridPosition);
        Renderer.color = team.TeamColor;
        GridManager.Instance.Register(this, GridPosition);
    }

    private void Update()
    {
        if (moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0 && queuedMove.HasValue)
            {
                TryMove(queuedMove.Value);
                queuedMove = null;
            }
        }
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        Vector2Int moveDir = new(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));
        if (moveDir == Vector2Int.zero) return;

        // Set facing
        FaceTo(dir);

        if (moveTimer > 0)
        {
            queuedMove = moveDir;
            return;
        }

        TryMove(moveDir);
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = GridPosition + direction;

        if (CanMoveToWithBlock(targetPos))
        {
            MoveWithBlock(direction);
        }

        moveTimer = MoveCooldown;
    }

    // FIX: Ignore the block currently held by this player in the relevant direction

    //<summary>
    //
    //</summary>


    private void MoveWithBlock(Vector2Int moveDir)
    {
        Move(moveDir);

        foreach (var block in grabbingBlocks)
            block.Move(moveDir);
        Debug.Log($"{this.name} move to {this.GridPosition}");
    }

    private void GrabBlock(Block block)
    {
        if (grabbingBlocks.Contains(block))
        {
            Debug.Log($"[Player] a player is already grabbing the block.");
            return;
        }
        grabbingBlocks.Add(block);
        block.SetGrabbedBy(this);
        Debug.Log($"[Player] a player grabbed a block");
    }

    private void ReleaseBlock(Block block)
    {
        if (!grabbingBlocks.Contains(block))
        {
            Debug.Log($"[Player] a block doesn't exist in grabbedBlocks");
            return;
        }
        block.SetGrabbedBy(null);
        grabbingBlocks.Remove(block);
        Debug.Log($"[Player] Released block in {block.GridPosition}");
    }

    private void OnGrabInput(InputAction.CallbackContext context)
    {
        List<Block> ReleasedBlocks = new();
        if (grabbingBlocks.Count > 0)
        {
            foreach (Block block in grabbingBlocks.ToList())
            {
                ReleaseBlock(block);
                ReleasedBlocks.Add(block);
            }
        }

        // Directional grab: Use facing direction
        Vector2Int faceDir = facing;
        Vector2Int watchingPos = GridPosition + faceDir;
        Block watchingBlock = GridManager.Instance.GetBlockAt(watchingPos);

        if (!ReleasedBlocks.Contains(watchingBlock)&&watchingBlock != null && watchingBlock.Team == Team && watchingBlock.GrabbingPlayer == null)
        {
            GrabBlock(watchingBlock);
        }
        else
        {
            Debug.Log("[Player] Cannot grab: No block, wrong team, or already grabbed.");
        }
    }


    /// <summary>
    /// Change the player's team and update any grabbed blocks' teams as well.
    /// </summary>
    public void ChangeTeam(Team newTeam)
    {
        Team = newTeam;
        Renderer.color = newTeam.TeamColor;
        foreach (var block in grabbingBlocks)
            block.SetTeam(newTeam);
    }

    public override string ToString() => $"Player({name}, {GridPosition})";

    private bool CanMoveToWithBlock(Vector2Int target)
    {
        Vector2Int prevPlayerPos = GridPosition;
        Vector2Int newPlayerPos = target;

        Vector2Int moveDir = newPlayerPos - prevPlayerPos;

        List<Vector2Int> prevBlockPosList = new();
        List<Vector2Int> newBlockPosList = new();

        foreach (var prevBlockPos in grabbingBlocks.Select(block => block.GridPosition).ToList())
        {
            prevBlockPosList.Add(prevBlockPos);
            Vector2Int newBlockPos = prevBlockPos + moveDir;
            newBlockPosList.Add(newBlockPos);
        }

        if (!prevBlockPosList.Contains(newPlayerPos) && GridManager.Instance.HasObstacleOccupants(newPlayerPos))
        {
            Debug.Log($"[Player] a player's move was blocked at {newPlayerPos}");
            return false;
        }

        foreach (var newBlockPos in newBlockPosList)
        {
            if (newBlockPos != prevPlayerPos && !prevBlockPosList.Contains(newBlockPos) && GridManager.Instance.HasObstacleOccupants(newBlockPos))
            {
                Debug.Log(newBlockPos);

                Debug.Log($"[Player] a block's move was blocked at {newPlayerPos}");
                return false;
            }
        }

        return true;

    }

    public void FaceTo(Vector2 dir)
    {
        if (dir == Vector2.zero) return;

        if (dir.x > 0 && dir.y == 0)
        {
            facing = Vector2Int.right;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (dir.x < 0 && dir.y == 0)
        {
            facing = Vector2Int.left;
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (dir.y > 0 && dir.x == 0)
        {
            facing = Vector2Int.up;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (dir.y < 0 && dir.x == 0)
        {
            facing = Vector2Int.down;
            transform.rotation = Quaternion.Euler(0, 0, 270);
        }
    }
}