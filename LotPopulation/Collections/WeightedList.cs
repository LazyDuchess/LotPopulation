using Sims3.Gameplay.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.Collections
{
    /// <summary>
    /// Represents a list of items. Each item has a weight associated with it, which can be used for randomization bias.
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    public class WeightedList<T> : IEnumerable<T>
    {
        public int Count => _unweightedList.Count;
        private readonly Dictionary<int, List<int>> _weightedIndices = new Dictionary<int, List<int>>();
        private readonly List<T> _weightedList = new List<T>();
        private readonly List<T> _unweightedList = new List<T>();

        public T this[int key]
        {
            get {
                return _unweightedList[key];
            }
            set
            {
                InternalSet(key, value);
            }
        }

        /// <summary>
        /// Modifies the weight of an item.
        /// </summary>
        /// <returns>True if the operation succeeded, False otherwise.</returns>
        public bool SetWeight(T item, int weight)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;
            return SetWeight(index, weight);
        }

        /// <summary>
        /// Modifies the weight of an item.
        /// </summary>
        /// <returns>True if the operation succeeded, False otherwise.</returns>
        public bool SetWeight(int index, int weight)
        {
            if (index < 0)
                return false;
            if (index >= _unweightedList.Count)
                return false;
            var originalItem = _unweightedList[index];
            RemoveAt(index);
            Add(originalItem, weight);
            return true;
        }

        /// <summary>
        /// Retrieves the weight of an item.
        /// </summary>
        /// <returns>Weight of the item, or 0 if it can't be found.</returns>
        public int GetWeight(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return 0;
            return GetWeight(index);
        }

        /// <summary>
        /// Retrieves the weight of an item.
        /// </summary>
        /// <returns>Weight of the item, or 0 if it can't be found.</returns>
        public int GetWeight(int index)
        {
            if (index < 0)
                return 0;
            if (index >= _unweightedList.Count)
                return 0;
            return _weightedIndices[index].Count;
        }

        /// <summary>
        /// Adds an item to the list with a weight of 1.
        /// </summary>
        public void Add(T item)
        {
            Add(item, 1);
        }

        /// <summary>
        /// Adds an item to the list with the specified weight.
        /// </summary>
        public void Add(T item, int weight)
        {
            if (weight <= 0)
                return;
            var indices = new List<int>();
            for(var i=0;i<weight;i++)
            {
                indices[i] = _weightedList.Count;
                _weightedList.Add(item);
            }
            var unweightedIndex = _unweightedList.Count;
            _unweightedList.Add(item);
            InternalAddIndices(unweightedIndex, indices);
        }

        /// <summary>
        /// Check if the list contains a specific item.
        /// </summary>
        /// <returns>Whether the list contains the item.</returns>
        public bool Contains(T item)
        {
            if (_unweightedList.Contains(item))
                return true;
            return false;
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <returns>True if the operation succeeded, False otherwise.</returns>
        public bool Remove(T item)
        {
            var index = _unweightedList.IndexOf(item);
            if (index < 0)
                return false;
            return RemoveAt(index);
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <returns>True if the operation succeeded, False otherwise.</returns>
        public bool RemoveAt(int index)
        {
            if (index < 0)
                return false;
            if (index >= _unweightedList.Count)
                return false;
            _unweightedList.RemoveAt(index);
            var indices = _weightedIndices[index];
            _weightedIndices.Remove(index);
            foreach (var ind in indices)
            {
                _weightedList.RemoveAt(ind);
            }
            return true;
        }

        /// <summary>
        /// Returns the index of the first occurrence of an item on the list.
        /// </summary>
        /// <returns>-1 if the item can't be found, index otherwise.</returns>
        public int IndexOf(T item)
        {
            return _unweightedList.IndexOf(item);
        }

        /// <summary>
        /// Returns this WeightedList as a normal List.
        /// </summary>
        /// <param name="weighted">Whether to return the list with duplicated items depending on their weight.</param>
        /// <returns>A list.</returns>
        public List<T> ToList(bool weighted)
        {
            if (weighted)
                return new List<T>(_weightedList);
            else
                return new List<T>(_unweightedList);
        }

        /// <summary>
        /// Get a random item from the list, items with higher weight are more likely to be picked.
        /// </summary>
        public T GetRandomItem()
        {
            return RandomUtil.GetRandomObjectFromList(_weightedList);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _unweightedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _unweightedList.GetEnumerator();
        }

        void InternalSet(int unweightedIndex, T item)
        {
            if (!_weightedIndices.TryGetValue(unweightedIndex, out List<int> weightedIndices))
                return;
            foreach(var index in weightedIndices)
            {
                _weightedList[index] = item;
            }
            _unweightedList[unweightedIndex] = item;
        }

        void InternalAddIndices(int unweightedIndex, List<int> weightedIndices)
        {
            if (!_weightedIndices.TryGetValue(unweightedIndex, out List<int> outIndices))
                outIndices = weightedIndices;
            else
                outIndices.AddRange(weightedIndices);
            _weightedIndices[unweightedIndex] = outIndices;
        }
    }
}
