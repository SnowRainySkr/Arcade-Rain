// SnowRainySkr create at 2025-02-19 12:49:42

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MemoryPack;

namespace Arcade.Utils.Collections.AugmentedIntervalTree {
	public abstract class SortedIntervalSet<T> : ICollection<T> where T : notnull {
		private sealed class IntervalSelector : IIntervalSelector<T, int> {
			public IntervalSelector(SortedIntervalSet<T> set) => Set = set;
			private SortedIntervalSet<T> Set { get; }

			public int GetStart(T item) => Set.StartTransform(item);

			public int GetEnd(T item) => Set.EndTransform(item);
		}

		protected abstract int StartTransform(T interval);

		protected abstract int EndTransform(T interval);

		private IntervalTree<T, int> Tree { get; }

		[MemoryPackInclude] protected List<T> AsList => Tree.ToList();

		[MemoryPackConstructor]
		protected SortedIntervalSet(List<T> asList) => Tree = new(asList, new IntervalSelector(this));

		protected int MaxEndPoint => Tree.MaxEndPoint;

		public void CopyTo(Array array, int arrayIndex) => Tree.CopyTo(array, arrayIndex);

		public void AddRange(IEnumerable<T> intervals) => Tree.AddRange(intervals);

		public T[] this[int point] => Tree[point];

		public T[] FindAt(int point) => Tree.FindAt(point);

		public bool ContainsPoint(int point) => Tree.ContainsPoint(point);

		public bool ContainsOverlappingInterval(T item) => Tree.ContainsOverlappingInterval(item);

		public T[] FindOverlapping(T item) => Tree.FindOverlapping(item);

		public IEnumerator<T> GetEnumerator() => Tree.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Tree).GetEnumerator();

		public void Add(T item) => Tree.Add(item);

		public void Clear() => Tree.Clear();

		public bool Contains(T item) => Tree.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => Tree.CopyTo(array, arrayIndex);

		public bool Remove(T item) => Tree.Remove(item);

		[MemoryPackIgnore] public int Count => Tree.Count;

		[MemoryPackIgnore] public bool IsReadOnly => Tree.IsReadOnly;
	}
}