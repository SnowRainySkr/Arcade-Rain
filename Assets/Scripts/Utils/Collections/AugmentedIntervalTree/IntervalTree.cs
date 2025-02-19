// SnowRainySkr create at 2025-02-19 09:29:20

/*
 * MIT License
 *
 * Copyright (c) 2013 brooknovak
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

// See in https://brooknovak.wordpress.com/2013/12/07/augmented-interval-tree-in-c/

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arcade.Utils.Collections.AugmentedIntervalTree {
	/// <summary>
	/// An interval tree that supports duplicate entries.
	/// </summary>
	/// <typeparam name="TInterval">The interval type</typeparam>
	/// <typeparam name="TPoint">The interval's start and end type</typeparam>
	/// <remarks>
	/// This interval tree is implemented as a balanced augmented AVL tree.
	/// Modifications are O(log n) typical case.
	/// Searches are O(log n) typical case.
	/// </remarks>
	internal sealed class IntervalTree<TInterval, TPoint> : ICollection<TInterval>, ICollection
		where TPoint : IComparable<TPoint> {
		private IntervalNode _root;
		private ulong        _modifications;

		private readonly IIntervalSelector<TInterval, TPoint> _intervalSelector;

		/// <summary>
		/// Default ctor required for XML serialization support
		/// </summary>
		private IntervalTree() => SyncRoot = new();

		public IntervalTree([NotNull] List<TInterval> sorted, [NotNull] IIntervalSelector<TInterval, TPoint> selector)
			: this(selector) {
			_root = GetSubTree(0, sorted.Count - 1);
			return;

			IntervalNode GetSubTree(int start, int end) {
				if (start > end) return null;
				var mid      = (start + end) / 2;
				var interval = sorted[mid];
				var hi       = selector.GetEnd(interval);
				var root = new IntervalNode(interval, selector.GetStart(interval), hi) {
					Left  = GetSubTree(start, mid - 1),
					Right = GetSubTree(mid + 1, end),
				};
				root.Height      = 1 + Math.Max(root.Left?.Height ?? 0, root.Right?.Height ?? 0);
				root.MaxEndPoint = root.Right is null ? hi : root.Right.MaxEndPoint;
				return root;
			}
		}

		private IntervalTree([NotNull] IIntervalSelector<TInterval, TPoint> intervalSelector) : this() {
			_intervalSelector = intervalSelector;
		}

		/// <summary>
		/// Returns the maximum end point in the entire collection.
		/// </summary>
		public TPoint MaxEndPoint {
			get {
				if (_root == null) {
					throw new InvalidOperationException("Cannot determine max end point for empty interval tree");
				}

				return _root.MaxEndPoint;
			}
		}

		#region IEnumerable, IEnumerable<T>

		IEnumerator IEnumerable.GetEnumerator() => new IntervalTreeEnumerator(this);

		public IEnumerator<TInterval> GetEnumerator() => new IntervalTreeEnumerator(this);

		#endregion

		#region ICollection

		public bool IsSynchronized => false;

		public object SyncRoot { get; }

		public void CopyTo(Array array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException(nameof(array));
			PerformCopy(arrayIndex, array.Length, (i, v) => array.SetValue(v, i));
		}

		#endregion

		#region ICollection<T>

		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public void CopyTo(TInterval[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException(nameof(array));
			PerformCopy(arrayIndex, array.Length, (i, v) => array[i] = v);
		}

		/// <summary>
		/// Tests if an item is contained in the tree.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>
		/// True if the item exists in the collection. 
		/// </returns>
		/// <remarks>
		/// This method uses the collection’s objects’ Equals and CompareTo methods on item to determine whether item exists.
		/// </remarks>
		public bool Contains(TInterval item) {
			if (ReferenceEquals(item, null)) throw new ArgumentNullException(nameof(item));
			return FindMatchingNodes(item).Any();
		}

		public void Clear() {
			SetRoot(null);
			Count = 0;
			_modifications++;
		}

		public void Add(TInterval item) {
			if (ReferenceEquals(item, null))
				throw new ArgumentNullException(nameof(item));

			var newNode = new IntervalNode(item, Start(item), End(item));

			if (_root == null) {
				SetRoot(newNode);
				Count = 1;
				_modifications++;
				return;
			}

			var node = _root;
			while (true) {
				var startCmp = newNode.Start.CompareTo(node.Start);
				if (startCmp <= 0) {
					if (startCmp == 0 && ReferenceEquals(node.Data, newNode.Data)) {
						throw new InvalidOperationException(
							"Cannot add the same item twice (object reference already exists in db)"
						);
					}

					if (node.Left == null) {
						node.Left = newNode;
						break;
					}

					node = node.Left;
				} else {
					if (node.Right == null) {
						node.Right = newNode;
						break;
					}

					node = node.Right;
				}
			}

			_modifications++;
			Count++;

			// Restructure tree to be balanced
			node = newNode;
			while (node != null) {
				node.UpdateHeight();
				node.UpdateMaxEndPoint();
				Rebalance(node);
				node = node.Parent;
			}
		}

		/// <summary>
		/// Removes an item.
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>True if an item was removed</returns>
		/// <remarks>
		/// This method uses the collection’s objects’ Equals and CompareTo methods on item to retrieve the existing item.
		/// If there are duplicates of the item, then object reference is used to remove.
		/// If <see cref="TInterval"/> is not a reference type, then the first found equal interval will be removed.
		/// </remarks>
		public bool Remove(TInterval item) {
			if (ReferenceEquals(item, null))
				throw new ArgumentNullException(nameof(item));

			if (_root == null)
				return false;

			var candidates = FindMatchingNodes(item).ToList();

			if (candidates.Count == 0)
				return false;

			IntervalNode toBeRemoved;
			if (candidates.Count == 1) {
				toBeRemoved = candidates[0];
			} else {
				toBeRemoved = candidates.SingleOrDefault(x => ReferenceEquals(x.Data, item)) ?? candidates[0];
			}

			var parent      = toBeRemoved.Parent;
			var isLeftChild = toBeRemoved.IsLeftChild;

			if (toBeRemoved.Left == null && toBeRemoved.Right == null) {
				if (parent != null) {
					if (isLeftChild)
						parent.Left = null;
					else
						parent.Right = null;

					Rebalance(parent);
				} else {
					SetRoot(null);
				}
			} else if (toBeRemoved.Right == null) {
				if (parent != null) {
					if (isLeftChild)
						parent.Left = toBeRemoved.Left;
					else
						parent.Right = toBeRemoved.Left;

					Rebalance(parent);
				} else {
					SetRoot(toBeRemoved.Left);
				}
			} else if (toBeRemoved.Left == null) {
				if (parent != null) {
					if (isLeftChild)
						parent.Left = toBeRemoved.Right;
					else
						parent.Right = toBeRemoved.Right;

					Rebalance(parent);
				} else {
					SetRoot(toBeRemoved.Right);
				}
			} else {
				IntervalNode replacement, replacementParent, temp;

				if (toBeRemoved.Balance > 0) {
					if (toBeRemoved.Left.Right == null) {
						replacement       = toBeRemoved.Left;
						replacement.Right = toBeRemoved.Right;
						temp              = replacement;
					} else {
						replacement = toBeRemoved.Left.Right;
						while (replacement.Right != null) {
							replacement = replacement.Right;
						}

						replacementParent       = replacement.Parent;
						replacementParent.Right = replacement.Left;

						temp = replacementParent;

						replacement.Left  = toBeRemoved.Left;
						replacement.Right = toBeRemoved.Right;
					}
				} else {
					if (toBeRemoved.Right.Left == null) {
						replacement      = toBeRemoved.Right;
						replacement.Left = toBeRemoved.Left;
						temp             = replacement;
					} else {
						replacement = toBeRemoved.Right.Left;
						while (replacement.Left != null) {
							replacement = replacement.Left;
						}

						replacementParent      = replacement.Parent;
						replacementParent.Left = replacement.Right;

						temp = replacementParent;

						replacement.Left  = toBeRemoved.Left;
						replacement.Right = toBeRemoved.Right;
					}
				}

				if (parent != null) {
					if (isLeftChild)
						parent.Left = replacement;
					else
						parent.Right = replacement;
				} else {
					SetRoot(replacement);
				}

				Rebalance(temp);
			}

			toBeRemoved.Parent = null;
			Count--;
			_modifications++;
			return true;
		}

		#endregion

		#region Public methods

		public void AddRange([NotNull] IEnumerable<TInterval> intervals) {
			foreach (var interval in intervals) Add(interval);
		}

		[NotNull]
		public TInterval[] this[TPoint point] => FindAt(point);

		[NotNull]
		public TInterval[] FindAt([NotNull] TPoint point) {
			var found = new List<IntervalNode>();
			PerformStabbingQuery(_root, point, found);
			return found.Select(node => node.Data).ToArray();
		}

		public bool ContainsPoint([NotNull] TPoint point) => FindAt(point).Length != 0;

		public bool ContainsOverlappingInterval([NotNull] TInterval item) {
			return PerformStabbingQuery(_root, item).Count > 0;
		}

		[NotNull]
		public TInterval[] FindOverlapping([NotNull] TInterval item) {
			return PerformStabbingQuery(_root, item).Select(node => node.Data).ToArray();
		}

		#endregion

		#region Private methods

		private void PerformCopy(int arrayIndex, int arrayLength, Action<int, TInterval> setAtIndexDelegate) {
			if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			var i = arrayIndex;

			using var enumerator = GetEnumerator();
			while (enumerator.MoveNext()) {
				if (i >= arrayLength) {
					throw new ArgumentOutOfRangeException(
						nameof(arrayIndex), "Not enough elements in array to copy content into"
					);
				}

				setAtIndexDelegate(i, enumerator.Current);
				i++;
			}
		}

		private IEnumerable<IntervalNode> FindMatchingNodes(TInterval interval) {
			return PerformStabbingQuery(_root, interval).Where(node => node.Data.Equals(interval));
		}

		private void SetRoot(IntervalNode node) {
			_root = node;
			if (_root != null) _root.Parent = null;
		}

		private TPoint Start(TInterval interval) => _intervalSelector.GetStart(interval);

		private TPoint End(TInterval interval) => _intervalSelector.GetEnd(interval);

		private bool DoesIntervalContain(TInterval interval, TPoint point) {
			return point.CompareTo(Start(interval)) >= 0 && point.CompareTo(End(interval)) <= 0;
		}

		private bool DoIntervalsOverlap(TInterval interval, TInterval other) {
			return Start(interval).CompareTo(End(other)) <= 0 && End(interval).CompareTo(Start(other)) >= 0;
		}

		private void PerformStabbingQuery(IntervalNode node, TPoint point, List<IntervalNode> result) {
			while (true) {
				if (node == null) return;

				if (point.CompareTo(node.MaxEndPoint) > 0) return;

				if (node.Left != null) PerformStabbingQuery(node.Left, point, result);

				if (DoesIntervalContain(node.Data, point)) result.Add(node);

				if (point.CompareTo(node.Start) < 0) return;

				if (node.Right != null) {
					node = node.Right;
					continue;
				}

				break;
			}
		}

		private List<IntervalNode> PerformStabbingQuery(IntervalNode node, TInterval interval) {
			var result = new List<IntervalNode>();
			PerformStabbingQuery(node, interval, result);
			return result;
		}

		private void PerformStabbingQuery(IntervalNode node, TInterval interval, List<IntervalNode> result) {
			while (true) {
				if (node == null) return;

				if (Start(interval).CompareTo(node.MaxEndPoint) > 0) return;

				if (node.Left != null) PerformStabbingQuery(node.Left, interval, result);

				if (DoIntervalsOverlap(node.Data, interval)) result.Add(node);

				if (End(interval).CompareTo(node.Start) < 0) return;

				if (node.Right != null) {
					node = node.Right;
					continue;
				}

				break;
			}
		}

		private void Rebalance(IntervalNode node) {
			if (node.Balance > 1) {
				if (node.Left.Balance < 0)
					RotateLeft(node.Left);
				RotateRight(node);
			} else if (node.Balance < -1) {
				if (node.Right.Balance > 0)
					RotateRight(node.Right);
				RotateLeft(node);
			}
		}

		private void RotateLeft(IntervalNode node) {
			var parent          = node.Parent;
			var isNodeLeftChild = node.IsLeftChild;

			// Make node.Right the new root of this subtree (instead of node)
			var pivotNode = node.Right;
			node.Right     = pivotNode.Left;
			pivotNode.Left = node;

			if (parent != null) {
				if (isNodeLeftChild) {
					parent.Left = pivotNode;
				} else {
					parent.Right = pivotNode;
				}
			} else {
				SetRoot(pivotNode);
			}
		}

		private void RotateRight(IntervalNode node) {
			var parent          = node.Parent;
			var isNodeLeftChild = node.IsLeftChild;

			// Make node.Left the new root of this subtree (instead of node)
			var pivotNode = node.Left;
			node.Left       = pivotNode.Right;
			pivotNode.Right = node;

			if (parent != null) {
				if (isNodeLeftChild) {
					parent.Left = pivotNode;
				} else {
					parent.Right = pivotNode;
				}
			} else {
				SetRoot(pivotNode);
			}
		}

		#endregion

		#region Inner classes

		[Serializable]
		private class IntervalNode {
			private IntervalNode _left;
			private IntervalNode _right;
			public  IntervalNode Parent      { get; set; }
			public  TPoint       Start       { get; private set; }
			private TPoint       End         { get; set; }
			public  TInterval    Data        { get; private set; }
			public  int          Height      { get; set; }
			public  TPoint       MaxEndPoint { get; set; }

			public IntervalNode(TInterval data, TPoint start, TPoint end) {
				if (start.CompareTo(end) > 0) {
					throw new ArgumentOutOfRangeException(
						nameof(end), "The supplied interval has an invalid range, where start is greater than end"
					);
				}

				Data  = data;
				Start = start;
				End   = end;
				UpdateMaxEndPoint();
			}

			public IntervalNode Left {
				get => _left;
				set {
					_left = value;
					if (_left != null) _left.Parent = this;
					UpdateHeight();
					UpdateMaxEndPoint();
				}
			}

			public IntervalNode Right {
				get => _right;
				set {
					_right = value;
					if (_right != null) _right.Parent = this;
					UpdateHeight();
					UpdateMaxEndPoint();
				}
			}

			public int Balance {
				get {
					if (Left != null && Right != null)
						return Left.Height - Right.Height;
					if (Left != null)
						return Left.Height + 1;
					if (Right != null)
						return -(Right.Height + 1);
					return 0;
				}
			}

			public bool IsLeftChild => Parent != null && Parent.Left == this;

			public void UpdateHeight() {
				if (Left != null && Right != null)
					Height = Math.Max(Left.Height, Right.Height) + 1;
				else if (Left != null)
					Height = Left.Height + 1;
				else if (Right != null)
					Height = Right.Height + 1;
				else
					Height = 0;
			}

			private static TPoint Max(TPoint comp1, TPoint comp2) => comp1.CompareTo(comp2) > 0 ? comp1 : comp2;

			public void UpdateMaxEndPoint() {
				var max = End;
				if (Left != null)
					max = Max(max, Left.MaxEndPoint);
				if (Right != null)
					max = Max(max, Right.MaxEndPoint);
				MaxEndPoint = max;
			}

			public override string ToString() {
				return $"[{Start}, {End}], maxEnd = {MaxEndPoint}";
			}
		}

		private class IntervalTreeEnumerator : IEnumerator<TInterval> {
			private readonly ulong                           _modificationsAtCreation;
			private readonly IntervalTree<TInterval, TPoint> _tree;
			private readonly IntervalNode                    _startNode;
			private          IntervalNode                    _current;
			private          bool                            _hasVisitedCurrent;
			private          bool                            _hasVisitedRight;

			public IntervalTreeEnumerator(IntervalTree<TInterval, TPoint> tree) {
				_tree                    = tree;
				_modificationsAtCreation = tree._modifications;
				_startNode               = GetLeftMostDescendantOrSelf(tree._root);
				Reset();
			}

			public TInterval Current {
				get {
					if (_current == null)
						throw new InvalidOperationException("Enumeration has finished.");

					if (ReferenceEquals(_current, _startNode) && !_hasVisitedCurrent)
						throw new InvalidOperationException("Enumeration has not started.");

					return _current.Data;
				}
			}

			object IEnumerator.Current => Current;

			public void Reset() {
				if (_modificationsAtCreation != _tree._modifications)
					throw new InvalidOperationException("Collection was modified.");
				_current           = _startNode;
				_hasVisitedCurrent = false;
				_hasVisitedRight   = false;
			}

			public bool MoveNext() {
				if (_modificationsAtCreation != _tree._modifications)
					throw new InvalidOperationException("Collection was modified.");

				if (_tree._root == null)
					return false;

				// Visit this node
				if (!_hasVisitedCurrent) {
					_hasVisitedCurrent = true;
					return true;
				}

				// Go right, visit the right's left most descendant (or the right node itself)
				if (!_hasVisitedRight && _current.Right != null) {
					_current = _current.Right;
					MoveToLeftMostDescendant();
					_hasVisitedCurrent = true;
					_hasVisitedRight   = false;
					return true;
				}

				// Move upward
				do {
					var wasVisitingFromLeft = _current.IsLeftChild;
					_current = _current.Parent;
					if (!wasVisitingFromLeft) continue;
					_hasVisitedCurrent = false;
					_hasVisitedRight   = false;
					return MoveNext();
				} while (_current != null);

				return false;
			}

			private void MoveToLeftMostDescendant() {
				_current = GetLeftMostDescendantOrSelf(_current);
			}

			private static IntervalNode GetLeftMostDescendantOrSelf(IntervalNode node) {
				if (node == null)
					return null;
				while (node.Left != null) {
					node = node.Left;
				}

				return node;
			}

			public void Dispose() { }
		}

		#endregion
	}

	/// <summary>
	/// Selects interval start and end points for an object of type <see cref="TInterval"/>.
	/// </summary>
	/// <typeparam name="TInterval">The type containing interval data</typeparam>
	/// <typeparam name="TPoint">The type of the interval start and end points</typeparam>
	internal interface IIntervalSelector<in TInterval, out TPoint> where TPoint : IComparable<TPoint> {
		TPoint GetStart(TInterval item);

		TPoint GetEnd(TInterval item);
	}
}