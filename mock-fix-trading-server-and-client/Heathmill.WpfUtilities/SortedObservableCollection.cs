using System;
using System.Collections.Generic;
using System.Linq;

namespace Heathmill.WpfUtilities
{
    public class SortedObservableCollection<T> : RangeAddObservableCollection<T>
    {
        private readonly SortedList<T, int> _sorted = new SortedList<T, int>();

        private readonly IMarshallToUi _uiExecutive;

        public SortedObservableCollection(List<T> list) : this(list.AsEnumerable())
        {
        }

        // ReSharper disable DoNotCallOverridableMethodsInConstructor
        public SortedObservableCollection(IEnumerable<T> collection) : this()
        {
            foreach (T item in collection) InsertItem(0, item);
        }

        // ReSharper restore DoNotCallOverridableMethodsInConstructor

        public SortedObservableCollection()
        {
            _uiExecutive = new SimpleUiExecutive();
        }

        public new void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void Load(T[] items)
        {
            Clear();
            lock (_sorted) foreach (T item in items) _sorted.Add(item, 0);
            _uiExecutive.MarshallWait(() => base.AddRange(_sorted.Keys));
        }

        protected override void InsertItem(int index, T item)
        {
            int i = AddAndGetSortedIndex(item);
            _uiExecutive.MarshallWait(() => base.InsertItem(i, item));
        }

        protected override void RemoveItem(int index)
        {
            lock (_sorted) _sorted.RemoveAt(index);
            _uiExecutive.MarshallWait(() => base.RemoveItem(index));
        }

        protected override void ClearItems()
        {
            lock (_sorted) _sorted.Clear();
            _uiExecutive.MarshallWait(base.ClearItems);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotSupportedException();
        }

        public new void AddRange(IEnumerable<T> items)
        {
            throw new NotSupportedException();
        }

        protected override void SetItem(int index, T item)
        {
            T olditem = this[index];
            if (olditem.Equals(item)) return;
            int i = ReplaceAndGetSortedIndex(index, item);
            _uiExecutive.MarshallWait(() => MoveAndReplaceItem(index, i, item));
        }

        private int AddAndGetSortedIndex(T item)
        {
            lock (_sorted)
            {
                _sorted.Add(item, 0);
                return _sorted.IndexOfKey(item);
            }
        }

        private int ReplaceAndGetSortedIndex(int index, T item)
        {
            lock (_sorted)
            {
                _sorted.RemoveAt(index);
                _sorted.Add(item, 0);
                return _sorted.IndexOfKey(item);
            }
        }

        private void MoveAndReplaceItem(int oldIndex, int newIndex, T item)
        {
            if (newIndex != oldIndex) base.MoveItem(oldIndex, newIndex);
            base.SetItem(newIndex, item);
        }
    }
}
