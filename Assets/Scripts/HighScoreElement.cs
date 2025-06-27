using System;

[Serializable]
public class HighScoreElement
{
    public string playerName;
    public int score;

    public HighScoreElement(string name, int points)
    {
        this.playerName = name;
        this.score = points;
    }
}
