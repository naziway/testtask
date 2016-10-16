using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UIDemo.ViewModel
{
    public static class ObservableCollectionExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            var sorted = collection.OrderBy(x => x).ToList();
            for (var i = 0; i < sorted.Count(); ++i)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }
}
