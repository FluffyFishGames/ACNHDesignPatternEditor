namespace MyHorizons.History
{
    public interface IRedoable : IHistoryItem
    {
        HistoryState Redo(HistoryState previousState);
    }
}
