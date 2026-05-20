using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]

[DisallowMultipleComponent]

public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;

    private void Awake()
    {
        // Load components
        enemy = GetComponent<Enemy>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // Create wait for fixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // REset player reference position
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        MoveEnemy();
    }

    /// <summary>
    /// Use A* pathfinding to build a path to the player - and then move the enemy to each grid location on the path
    /// </summary>
    private void MoveEnemy()
    {
        // Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        // Check distance to player to see if enemy should start chasing
        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        // If the enemy is not close enough to the player (for chasing) then return.
        if (!chasePlayer)
        {
            return;
        }

        // If the movement cooldown timer is reached or the player has moved more than the required distance, then rebuild the enemy path and move the enemy
        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) > Settings.playerMoveDistanceToRebuildPath))
        {
            // Reset path rebuild cooldown timer
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            // Move the enemy using A* pathfinding - trigger rebuild of path to player
            CreatePath();

            // IF a path a is found, move the enemy
            if (movementSteps != null)
            {
                if (moveEnemyRoutine != null)
                {
                    // Trigger idle event
                    enemy.idleEvent.CallIdleEvent();
                    StopCoroutine(moveEnemyRoutine);
                }

                // Move enemy along the path using a coroutine
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
            }
        }
    }

    /// <summary>
    /// Use the A* static class to create a path for the enemy
    /// </summary>
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Get enemy position on the grid
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);
        
        // Get player position on the grid
        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

        // Build a path for the enemy to move on
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        // Take off first step on path - this is the grid square that the enemy is already on
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            // Trigger idle event as there is no path
            enemy.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Get the nearest position to the player that is NOT on an obstacle
    /// </summary>
    /// <param name="currentRoom"></param>
    /// <returns>Cell position of player if no obstacle, otherwise nearest non-obstacle cell position</returns>
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        // Get the position of the player in the world
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        // Convert the player world position to a cell position
        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        // Adjust the position of the player based on the bounds of the room template
        Vector2Int adjustedPlayerCellPosition = new Vector2Int
        (
            playerCellPosition.x - currentRoom.templateLowerBounds.x, 
            playerCellPosition.y - currentRoom.templateLowerBounds.y
        );

        // If the cell in the room has a movement penalty of 0 (as decided in the Settings script) then that cell "is" an obstacle
        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        // If the player is not on a cell marked as an obstacle (movement penalty != 0) then return that position
        if (obstacle != 0)
        {
            return playerCellPosition;
        }
        // Find the nearest surrounding cell that is not an obstacle. Think of half-height collision tiles.
        else
        {
            // Check all adjacent (orthogonally and diagonally) cells
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // If on the current cell (which we know is an obstacle, skip it)
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    // Try-catch block in case of index-out-of-bounds error
                    try
                    {
                        obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + i, adjustedPlayerCellPosition.y + j];
                        if (obstacle != 0)
                        {
                            return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0);
                        }
                    } 
                    catch
                    {
                        continue;
                    }
                }
            }

            // No non-obstacle cells surrounding the player so just return the player position
            return playerCellPosition;
        }
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path
    /// </summary>
    /// <param name="movementSteps"></param>
    /// <returns></returns>
    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        // Should/can this be a foreach loop?
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                // Trigger enemy movement event
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition - transform.position).normalized);

                yield return waitForFixedUpdate; // Moving the enemy using 2D physics so we need to wait until the next fixed update
            }

            yield return waitForFixedUpdate;
        }

        // End of the path steps, trigger idle event for enemy
        enemy.idleEvent.CallIdleEvent();
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
