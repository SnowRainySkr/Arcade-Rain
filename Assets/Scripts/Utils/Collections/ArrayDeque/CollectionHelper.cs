// SnowRainySkr create at 2025-02-18 19:08:23

/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Stephen Cleary
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Utils.Collections.ArrayDeque {
	internal static class CollectionHelpers {
		public static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source) => source switch {
			IReadOnlyCollection<T> result    => result,
			ICollection<T> collection        => new CollectionWrapper<T>(collection),
			ICollection nongenericCollection => new NongenericCollectionWrapper<T>(nongenericCollection),
			_                                => new List<T>(source),
		};

		private sealed class NongenericCollectionWrapper<T> : IReadOnlyCollection<T> {
			private readonly ICollection _collection;

			public NongenericCollectionWrapper(ICollection collection) => _collection = collection;

			public int Count => _collection.Count;

			public IEnumerator<T> GetEnumerator() => _collection.Cast<T>().GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
		}

		private sealed class CollectionWrapper<T> : IReadOnlyCollection<T> {
			private readonly ICollection<T> _collection;

			public CollectionWrapper(ICollection<T> collection) => _collection = collection;

			public int Count => _collection.Count;

			public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
		}
	}
}