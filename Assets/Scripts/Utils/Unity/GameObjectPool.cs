// SnowRainySkr create at 2025-02-19 18:54:21

using System.Collections.Generic;
using UnityEngine;

namespace Arcade.Utils.Unity {
	public sealed class GameObjectPool<T> where T : Component {
		private T         Prefab { get; }
		private Transform Layer  { get; }
		private List<T>   Pool   { get; }

		public GameObjectPool(T prefab, Transform layer) {
			Prefab = prefab;
			Layer  = layer;
			Pool   = new();
		}

		public T Get() {
			if (Pool.Count is 0) {
				for (var i = 0; i < 10; i++) Pool.Add(Object.Instantiate(Prefab, Layer));
			}

			var result = Pool[^1];
			Pool.RemoveAt(Pool.Count - 1);
			return result;
		}

		public void Return(T item) => Pool.Add(item);

		public IEnumerator<T> GetEnumerator() => Pool.GetEnumerator();
	}
}