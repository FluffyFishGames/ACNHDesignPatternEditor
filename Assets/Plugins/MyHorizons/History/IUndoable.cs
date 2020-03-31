namespace MyHorizons.History
{
    public interface IUndoable : IHistoryItem
    {
        HistoryState Undo(HistoryState previousState);
    }
}
