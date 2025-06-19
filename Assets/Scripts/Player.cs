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

    private Vector2Int FaceDir = Vector2Int.right;
    private float moveTimer;
    private Vector2Int? queuedMove;
    private Block grabbingBlock = null;

    // Input Actions
    private PlayerInputActions inputActions;

    // Cardinal directions
    static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        inputActions = new PlayerInputActions();
    }

    private void Start()//player
    {
        Vector2 startPos = new(transform.position.x, transform.position.y);
        Vector2Int roundedStartPos = Vector2Int.RoundToInt(startPos);
        Initialize(roundedStartPos, Team);
    }

    private void OnEnable()//input
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMoveInput;
        inputActions.Player.Grab.performed += OnGrabInput;
    }

    private void OnDisable()//input
    {
        inputActions.Player.Move.performed -= OnMoveInput;
        inputActions.Player.Grab.performed -= OnGrabInput;
        inputActions.Disable();
    }

    public void Initialize(Vector2Int startPos, Team team)//player
    {
        GridPos = startPos;
        Team = team;
        transform.position = GridManager.GridToWorld(GridPos);
        Renderer.color = team.TeamColor;
        GridManager.Instance.Register(this, GridPos);
        Debug.Log($"[Playera player successfully initialized]");
    }

    private void Update()//input
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

    private void OnMoveInput(InputAction.CallbackContext context)//input
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

    private void TryMove(Vector2Int moveDir)//actionmove
    {
        if (CanMoveWithBlock(moveDir))
        {
            MoveWithBlock(moveDir);
        }

        moveTimer = MoveCooldown;
    }

    // FIX: Ignore the block currently held by this player in the relevant direction

    //<summary>
    //
    //</summary>


    private void MoveWithBlock(Vector2Int moveDir)//actionmove
    {
        Move(moveDir);
        if (grabbingBlock != null)grabbingBlock.Move(moveDir);
        Debug.Log($"{this.name} move to {this.GridPos}");
    }

    private void GrabBlock(Block block)//actiongrab
    {
        if (grabbingBlock != null)
        {
            Debug.Log($"[Player] a player is already grabbing the block.");
            return;
        }
        grabbingBlock = block;
        block.SetGrabbedBy(this);
        Debug.Log($"[Player] a player grabbed a block");
    }

    private void ReleaseBlock(Block block)//actiongrab
    {
        if (grabbingBlock != block)
        {
            Debug.Log($"[Player] the block you tried to release is not grabbed");
            return;
        }
        block.SetGrabbedBy(null);
        grabbingBlock = null;
        Debug.Log($"[Player] Released block in {block.GridPos}");
    }

    private void OnGrabInput(InputAction.CallbackContext context)//input
    {
        Block releasedBlock = null;
        if (grabbingBlock != null)
        {
            releasedBlock = grabbingBlock;
            ReleaseBlock(releasedBlock);
        }

        // Directional grab: Use facing direction
        Vector2Int watchingPos = GridPos + FaceDir;
        Block watchingBlock = GridManager.Instance.GetBlockAt(watchingPos);

        if (releasedBlock != watchingBlock && watchingBlock != null && watchingBlock.Team == Team && watchingBlock.GrabbingPlayer == null)
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
    public void ChangeTeam(Team newTeam)//teammanager
    {
        Team = newTeam;
        Renderer.color = newTeam.TeamColor;
        grabbingBlock.Team = newTeam;
    }

    private bool CanMoveWithBlock(Vector2Int moveDir)//actionmove
    {
        if (grabbingBlock == null)
        {
            Vector2Int prevPlayerPos = GridPos;
            Vector2Int newPlayerPos = GridPos + moveDir;

            if (GridManager.Instance.HasObstacleOccupants(newPlayerPos))
            {
                Debug.Log($"[Player] a player's move was blocked at {newPlayerPos}");
                return false;
            }
            return true;
        }

        else
        {
            Vector2Int prevPlayerPos = GridPos;
            Vector2Int newPlayerPos = GridPos + moveDir;

            Vector2Int prevBlockPos = grabbingBlock.GridPos;
            Vector2Int newBlockPos = grabbingBlock.GridPos + moveDir;

            if (newPlayerPos != prevBlockPos && GridManager.Instance.HasObstacleOccupants(newPlayerPos))
            {
                Debug.Log($"[Player] a player's move was blocked at {newPlayerPos}");
                return false;
            }

            if (newBlockPos != prevPlayerPos && GridManager.Instance.HasObstacleOccupants(newBlockPos))
            {
                Debug.Log($"[Player] a block's move was blocked at {newPlayerPos}");
                return false;
            }
            return true;
        }
    }

    public void FaceTo(Vector2 dir)//actionmove
    {
        if (dir == Vector2.zero) return;

        if (dir.x > 0 && dir.y == 0)
        {
            FaceDir = Vector2Int.right;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (dir.x < 0 && dir.y == 0)
        {
            FaceDir = Vector2Int.left;
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (dir.y > 0 && dir.x == 0)
        {
            FaceDir = Vector2Int.up;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (dir.y < 0 && dir.x == 0)
        {
            FaceDir = Vector2Int.down;
            transform.rotation = Quaternion.Euler(0, 0, 270);
        }
    }
}