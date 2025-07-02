using System;

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;

    public ScoreEntry(string name, int points)
    {
        this.playerName = name;
        this.score = points;
    }
}

// A wrapper class to hold a list of score entries.
// JsonUtility cannot directly serialize a root-level list, so we need this.
[Serializable]
public class HiScoreList
{
    public ScoreEntry[] scores;
}
