public class GameState
{
    public bool FirstHalf;
    public float TimeElapsed;
    public int ScoreBlue;
    public int ScoreYellow;

    public void Reset()
    {
        FirstHalf = true;
        TimeElapsed = 0f;
        ScoreBlue = 0;
        ScoreYellow = 0;
    }
}