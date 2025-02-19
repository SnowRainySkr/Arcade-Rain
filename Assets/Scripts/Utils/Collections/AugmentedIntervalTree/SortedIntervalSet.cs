// SnowRainySkr create at 2025-02-19 12:49:42

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MemoryPack;

namespace Arcade.Utils.Collections.AugmentedIntervalTree {
	/// <summary>
	/// Represents a sorted collection of intervals based on an augmented interval tree structure.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the set, which must be non-nullable.</typeparam>
	public abstract class SortedIntervalSet<T> : ICollection<T> where T : notnull {
		private sealed class IntervalSelector : IIntervalSelector<T, int> {
			public IntervalSelector(SortedIntervalSet<T> set) => Set = set;
			private SortedIntervalSet<T> Set { get; }

			public int GetStart(T item) => Set.StartTransform(item);

			public int GetEnd(T item) => Set.EndTransform(item);
		}

		/// <summary>
		/// When overridden in a derived class, transforms the specified interval into its start point.
		/// </summary>
		/// <param name="interval">The interval to transform.</param>
		/// <returns>The start point of the interval.</returns>
		protected abstract int StartTransform(T interval);

		/// <summary>
		/// When overridden in a derived class, transforms the specified interval into its end point.
		/// </summary>
		/// <param name="interval">The interval to transform.</param>
		/// <returns>The end point of the interval.</returns>
		protected abstract int EndTransform(T interval);

		private IntervalTree<T, int> Tree { get; }

		[MemoryPackInclude] protected List<T> AsList => Tree.ToList();

		/// <summary>
		/// Initializes a new instance of the SortedIntervalSet class with the specified list of intervals.
		/// </summary>
		/// <param name="asList">The list of intervals to initialize the set with.</param>
		[MemoryPackConstructor]
		protected SortedIntervalSet(List<T> asList) => Tree = new(asList, new IntervalSelector(this));

		/// <summary>
		/// Gets the maximum end point of all intervals in the set.
		/// </summary>
		public int MaxEndPoint => Tree.MaxEndPoint;

		public void CopyTo(Array array, int arrayIndex) => Tree.CopyTo(array, arrayIndex);

		public void AddRange(IEnumerable<T> intervals) => Tree.AddRange(intervals);

		/// <summary>
		/// Gets the intervals that overlap with the specified point.
		/// </summary>
		/// <param name="point">The point to find overlapping intervals for.</param>
		/// <returns>An array of intervals that overlap with the specified point.</returns>
		public T[] this[int point] => Tree[point];

		/// <summary>
		/// Gets the intervals that overlap with the specified point.
		/// </summary>
		/// <param name="point">The point to find overlapping intervals for.</param>
		/// <returns>An array of intervals that overlap with the specified point.</returns>
		public T[] FindAt(int point) => Tree.FindAt(point);

		public bool ContainsPoint(int point) => Tree.ContainsPoint(point);
		
		public bool ContainsOverlappingInterval(T interval) => Tree.ContainsOverlappingInterval(interval);

		/// <summary>
		/// Finds all intervals in the set that intersect with the specified interval.
		/// </summary>
		/// <param name="interval">The interval to check for intersections.</param>
		/// <returns>A collection of intervals that intersect with the specified interval.</returns>
		/// <remarks>
		/// The intersection search operation has a time complexity of O(log n + m),
		/// where n is the number of elements in the set,
		/// and m is the number of intervals in the result.
		/// </remarks>
		public T[] FindOverlapping(T interval) => Tree.FindOverlapping(interval);

		public IEnumerator<T> GetEnumerator() => Tree.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Tree).GetEnumerator();

		/// <summary>
		/// Adds an item to the <see cref="SortedIntervalSet{T}"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="SortedIntervalSet{T}"/>.</param>
		/// <remarks>
		/// The add operation has a time complexity of O(log n), where n is the number of elements in the set.
		/// </remarks>
		public void Add(T item) => Tree.Add(item);

		public void Clear() => Tree.Clear();

		/// <summary>
		/// Determines whether the <see cref="SortedIntervalSet{T}"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="SortedIntervalSet{T}"/>.</param>
		/// <returns>True if <paramref name="item"/> is found in the <see cref="SortedIntervalSet{T}"/>.</returns>
		/// <remarks>
		/// This method uses the collection’s objects’ Equals and CompareTo methods on item to determine whether item exists.
		/// </remarks>
		public bool Contains(T item) => Tree.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => Tree.CopyTo(array, arrayIndex);

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="SortedIntervalSet{T}"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="SortedIntervalSet{T}"/>.</param>
		/// <returns>true if <paramref name="item"/> was successfully removed from the <see cref="SortedIntervalSet{T}"/>; otherwise, false.</returns>
		/// <remarks>
		/// The remove operation has a time complexity of O(log n), where n is the number of elements in the set.
		/// </remarks>
		public bool Remove(T item) => Tree.Remove(item);

		[MemoryPackIgnore] public int Count => Tree.Count;

		[MemoryPackIgnore] public bool IsReadOnly => Tree.IsReadOnly;
	}
}