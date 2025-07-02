using UnityEngine;
using System.IO;

public static class HiScoreUtil
{
    public static int GetTopScore()
    {
        string path = Path.Combine(Application.persistentDataPath, "hiscores.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            HiScoreList list = JsonUtility.FromJson<HiScoreList>(json);

            // Check if there are any scores in the list before accessing.
            if (list != null && list.scores.Length > 0)
            {
                // The list is already sorted by the HiScoreManager, so the first entry is the highest.
                return list.scores[0].score;
            }
        }

        // Return 0 if no file or no scores exist.
        return 0;
    }
}
