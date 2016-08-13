using System;
using System.Collections.Generic;
using System.Text;

namespace CustomFontDialog
{
    /// <summary>
    /// A custom collection for maintaining recently used lists of any kind. For example, recently used fonts, color etc.
    /// List with limited size which is given by MaxSize. As list grows beyond MaxSize, oldest item is removed.
    /// New items are added at the top of the list (at index 0), existing items move down.
    /// If added item is already there in the list, it is moved to the top (at index 0).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecentlyUsedList<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxSize">As list grows longer than max size, oldest item will be dropped.</param>
        public RecentlyUsedList(int maxSize)
        {
            this.maxSize = maxSize;
        }

        private List<T> list = new List<T>();

        public T this[int i]
        {
            get
            {
                return list[i];
            }            
        }

        private int maxSize;

        public int MaxSize
        {
            get
            {
                return maxSize;
            }                      
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public void Add(T item)
        {
            int i = list.IndexOf(item);
            if (i != -1)    list.RemoveAt(i);

            if (list.Count == MaxSize) list.RemoveAt(list.Count - 1);

            list.Insert(0, item);
        }
    
    }
}
