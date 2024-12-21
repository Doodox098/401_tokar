namespace Contracts
{
    public class EvolutionParameters
    {
        public int Rounds { get; set; }
        public int Courts { get; set; }
        public int Players { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }
        public int PopulationSize { get; set; }
    }
}