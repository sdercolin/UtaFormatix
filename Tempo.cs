namespace UtaFormatix
{
    public partial class Data
    {
        public class Tempo
        {
            public Tempo()
            {
            }

            public Tempo(Tempo tempo)
            {
                PosTick = tempo.PosTick;
                BpmTimes100 = tempo.BpmTimes100;
            }

            public int PosTick;
            public int BpmTimes100;
            public double Bpm => ((double)BpmTimes100) / 100;
        }
    }
}