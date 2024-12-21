namespace Contracts
{
    public record EvolutionResult
    {
        public EvolutionResult(Dictionary<int, SortedSet<int>>[] table, (int, int) rank)
        {
            Table = table;
            Rank = rank;
        }
        public Dictionary<int, SortedSet<int>>[] Table { get; }
        public (int, int) Rank { get; }
    }
}