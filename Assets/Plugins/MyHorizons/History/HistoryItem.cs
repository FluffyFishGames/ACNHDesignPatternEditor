namespace MyHorizons.History
{
    public enum HistoryResult
    {
        Failed = -1,
        Success = 0
    }

    public interface IHistoryItem
    {
        string HistoryDescription { get; }
    }

    public sealed class HistoryState
    {
        public readonly int Index;
        public readonly object Object;

        public HistoryState(object obj)
        {
            Index = -1;
            Object = obj;
        }

        public HistoryState(int index, object obj)
        {
            Index = index;
            Object = obj;
        }
    }
}
