namespace Contracts
{
    public record EvolutionResult
    {
        public EvolutionResult(Dictionary<int, SortedSet<int>>[] table, string rank)
        {
            Table = table;
            Rank = rank;
        }
        public Dictionary<int, SortedSet<int>>[] Table { get; }
        public string Rank { get; }
    }
}