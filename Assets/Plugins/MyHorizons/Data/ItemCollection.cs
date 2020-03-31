using MyHorizons.History;

namespace MyHorizons.Data
{
    public class ItemCollection : Changeable, IUndoable, IRedoable
    {
        private Item[] Items;
        public string HistoryDescription { get; protected set; } = "Item Changed";

        public ItemCollection(Item[] items) => Items = items;
        public ItemCollection(int capacity) => Items = new Item[capacity];
        public int Count => Items.Length;

        public Item this[int key]
        {
            get => Items[key];
            set => SetProperty(ref Items[key], value);
        }

        public bool SetItems(Item[] items)
        {
            if (items == null) return false;

            Items = items;
            return true;
        }

        public HistoryState Undo(HistoryState state)
        {
            if (state.Index < 0 || state.Index >= Count || !(state.Object is Item item))
                return null;
            var newState = new HistoryState(state.Index, this[state.Index]);
            this[state.Index] = item;
            return newState;
        }

        public HistoryState Redo(HistoryState state) => Undo(state); // Same logic.
    }
}
