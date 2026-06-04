using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    public struct ChanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    private int ratioValueTotal = 0;
    private List<ChanceBoundaries> chanceBoundariesList = new List<ChanceBoundaries>();
    private List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList;

    public RandomSpawnableObject(List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList)
    {
        this.spawnableObjectsByLevelList = spawnableObjectsByLevelList;
    }

    /// <summary>
    /// Get a singular item
    /// </summary>
    /// <returns>The random spawnable object, default value if no spawnable object was found</returns>
    public T GetItem()
    {
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        T spawnableObject = default;

        // Loop through each of the spawnable objects by level
        foreach (SpawnableObjectsByLevel<T> spawnableObjectsByLevel in spawnableObjectsByLevelList)
        {
            // Check for current level
            if (spawnableObjectsByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                // Loop through the list of spawnable object ratios
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectsByLevel.spawnableObjectRatioList)
                {
                    // Set the new lower boundary
                    int lowerBoundary = upperBoundary + 1;

                    // Set the upper boundary to the lower boundary plus the ratio of the spawnable object
                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;

                    // Set the new ratio total value
                    ratioValueTotal += spawnableObjectRatio.ratio;

                    // Create a new entry in the chance boundaries list for the current spawnable object
                    chanceBoundariesList.Add(new ChanceBoundaries()
                    {
                        spawnableObject = spawnableObjectRatio.dungeonObject,
                        lowBoundaryValue = lowerBoundary,
                        highBoundaryValue = upperBoundary
                    });
                }
            }
        }

        // If the list is empty, return the default for the type
        if (chanceBoundariesList.Count == 0)
        {
            return default;
        }

        int lookUpValue = Random.Range(0, ratioValueTotal);

        // Loop through each of the entries in the chance boundaries list
        foreach (ChanceBoundaries spawnChance in chanceBoundariesList)
        {
            // If the random value is within the current spawnable object's boundaries
            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                spawnableObject = spawnChance.spawnableObject;
                break;
            }
        }

        // Return the spawnable object, if none is found then the default value for the type is returned
        return spawnableObject;
    }
}
