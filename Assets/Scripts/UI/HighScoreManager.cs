using System.IO; // File I/O
using System.Runtime.Serialization.Formatters.Binary; // Binary formatter
using UnityEngine;

public class HighScoreManager : SingletonMonobehavior<HighScoreManager>
{
    private HighScores highScores = new HighScores();

    protected override void Awake()
    {
        base.Awake();

        LoadScores();
    }

    /// <summary>
    /// Load scores from disk
    /// </summary>
    private void LoadScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        // Check if the high scores data file already exists
        if (File.Exists(Application.persistentDataPath + "/DungeonGunnerHighScores.dat"))
        {
            // Clear score list so we can make sure our list is freshly updated
            ClearScoreList();

            // Open high scores file
            FileStream fs = File.OpenRead(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

            // Allocate the deserialized high scores (must cast to a HighScores object!)
            highScores = (HighScores)bf.Deserialize(fs);

            fs.Close();
        }
    }

    /// <summary>
    /// Clear highscores list
    /// </summary>
    private void ClearScoreList()
    {
        highScores.scoreList.Clear();
    }

    /// <summary>
    /// Add score to high scores list
    /// </summary>
    /// <param name="score"></param>
    /// <param name="rank"></param>
    public void AddScore(Score score, int rank)
    {
        // Add score to high scores list
        highScores.scoreList.Insert(rank - 1, score);

        // Check if the number of high scores saved is now greater than allowed
        if (highScores.scoreList.Count > Settings.numberOfHighScoresToSave)
        {
            // Remove the lowest score in the high scores list
            highScores.scoreList.RemoveAt(Settings.numberOfHighScoresToSave);
        }

        SaveScores();
    }

    /// <summary>
    /// Save scores to disk
    /// </summary>
    private void SaveScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        // Create data file (or truncate and overwrite) for high scores storage
        FileStream fs = File.Create(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

        // Serialize the highscores in binary
        bf.Serialize(fs, highScores);

        fs.Close();
    }

    /// <summary>
    /// Get high scores
    /// </summary>
    /// <returns>HighScores object</returns>
    public HighScores GetHighScores()
    {
        return highScores;
    }

    /// <summary>
    /// Get the rank of the player score compared to the other high scores
    /// </summary>
    /// <param name="playerScore"></param>
    /// <returns>0 if score is not higher than any other score, otherwise 1-indexed rank index</returns>
    public int GetRank(long playerScore)
    {
        // Check if there are no scores in the high scores list
        if (highScores.scoreList.Count == 0)
        {
            // Return rank of 1 (as the list has not high scores yet!)
            return 1;
        }

        // Temp variable to store player rank
        int index = 0;

        // Loop through the high scores list
        for (int i = 0; i < highScores.scoreList.Count; i++)
        {
            // Increment player rank (where the "highest" player rank is 1 and the "lowest" player rank is 100)
            index++;

            // Check if the player score is higher than the current score iteration
            if (playerScore >= highScores.scoreList[i].playerScore)
            {
                // Return index of player rank (based on 1-index format)
                return index;
            }
        }

        // Check if the high scores list is not yet full
        if (highScores.scoreList.Count < Settings.numberOfHighScoresToSave)
        {
            // The rank for that player score is at the bottom (AKA last)
            return index + 1;
        }

        // Score not high enough to make it into the high scores list
        return 0;
    }
}
