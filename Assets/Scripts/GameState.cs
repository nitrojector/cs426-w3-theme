using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Unknown,
    Blue,
    Yellow
}

public class GameState
{
    public const Team FirstHalfStriker = Team.Blue;
    public const int roundsPerHalf = 5;
    
    public bool FirstHalf;
    public int roundNo = 0;
    public bool RoundInProgress = true;
    public bool RoundEnd = false;

    public Dictionary<Team, int[]> TeamRecords = new Dictionary<Team, int[]>
    {
        { Team.Blue, new int[roundsPerHalf] },
        { Team.Yellow, new int[roundsPerHalf] }
    };

    public Team OtherTeam(Team team)
    {
        return team switch
        {
            Team.Blue => Team.Yellow,
            Team.Yellow => Team.Blue,
            _ => Team.Unknown
        };
    }
    
    public bool IsGameOver()
    {
        return roundNo >= roundsPerHalf * 2;
    }

    public void SetRoundWinner(Team winner)
    {
        if (!RoundInProgress)
            return;
        
        TeamRecords[FirstHalf ? FirstHalfStriker : OtherTeam(FirstHalfStriker)][roundNo] =
            winner == (FirstHalf ? FirstHalfStriker : OtherTeam(FirstHalfStriker)) ? 1 : -1;
        
        RoundInProgress = false;
        RoundEnd = true;
    }

    public int ScoreOf(Team team)
    {
        int score = 0;
        for (int i = 0; i < roundsPerHalf; i++)
        {
            if (TeamRecords[team][i] == 1)
                score += 1;
        }
        return score;
    }
    
    /// <summary>
    /// Returns winner based on current standings
    /// </summary>
    /// <returns></returns>
    public Team GetWinner()
    {
        if (ScoreOf(Team.Blue) > ScoreOf(Team.Yellow))
            return Team.Blue;
        if (ScoreOf(Team.Blue) < ScoreOf(Team.Yellow))
            return Team.Yellow;
        return Team.Unknown;
    }
    
    public void AdvanceRound()
    {
        roundNo += 1;
        if (FirstHalf && roundNo >= roundsPerHalf)
        {
            FirstHalf = false;
            roundNo = 0;
        }
    }

    public void Reset()
    {
        FirstHalf = true;
        roundNo = 0;
        RoundInProgress = true;
        TeamRecords[Team.Blue] = new int[roundsPerHalf];
        TeamRecords[Team.Yellow] = new int[roundsPerHalf];
    }
}