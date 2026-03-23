using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPoolTest : MonoBehaviour
{
    [SerializeField] private EnemyAnimationDetails[] enemyAnimationDetailsArray;
    [SerializeField] GameObject enemyExamplePrefab;
    private float timer = 1f;

    [System.Serializable]
    public struct EnemyAnimationDetails
    {
        public RuntimeAnimatorController animatorController;
        public Color spriteColor;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        // Spawn random enemy sprite every second
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            GetEnemyExample();
            timer = 1f;
        }
    }

    /// <summary>
    /// Puts an enemy in the current room at a random position on one of the spawn points
    /// </summary>
    private void GetEnemyExample()
    {
        // Current room
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        // Calculate random spawn position within room bounds
        Vector3 spawnPosition = new Vector3(Random.Range(currentRoom.lowerBounds.x, currentRoom.upperBounds.x), Random.Range(currentRoom.lowerBounds.y, currentRoom.upperBounds.y), 0f);

        // Gets an enemy animation component from object pool queue
        EnemyAnimation enemyAnimation = (EnemyAnimation)PoolManager.Instance.ReuseComponent(enemyExamplePrefab, HelperUtilities.GetSpawnPositionNearestToPlayer(spawnPosition), Quaternion.identity);

        // Calculate random index for enemy animation details
        int randomIndex = Random.Range(0, enemyAnimationDetailsArray.Length);

        // Set the gameobject to true
        enemyAnimation.gameObject.SetActive(true);

        // Set an animator controller and color using random index
        enemyAnimation.SetAnimation(enemyAnimationDetailsArray[randomIndex].animatorController, enemyAnimationDetailsArray[randomIndex].spriteColor);

    }
}
