namespace DefaultNamespace
{
    public class GameState
    {
        public float TimeElapsed;
        public int ScoreGoalie;
        public int ScoreStriker;

        public void Reset()
        {
            TimeElapsed = 0f;
            ScoreGoalie = 0;
            ScoreStriker = 0;
        }
    }
}