// SnowRainySkr create at 2025-02-17 08:22:39

using System.IO;
using Cysharp.Threading.Tasks;
using MemoryPack;

namespace Arcade.Utils.IO {
	public static class Serialization {
		public static void ToBinFile<T>(this T obj, string path) where T : notnull {
			try {
				File.WriteAllBytes(path, MemoryPackSerializer.Serialize(obj));
			}
			catch (IOException) { }
		}

		public static async UniTask ToBinFileAsync<T>(this T obj, string path) where T : notnull {
			try {
				await using var outputStream = new StreamWriter(path);
				await MemoryPackSerializer.SerializeAsync(outputStream.BaseStream, obj);
			}
			catch (IOException) { }
		}

		public static T? FromBinFile<T>(this string path) {
			try {
				return MemoryPackSerializer.Deserialize<T>(File.ReadAllBytes(path));
			}
			catch (IOException) {
				return default;
			}
		}

		public static async UniTask<T?> FromBinFileAsync<T>(this string path) {
			try {
				using var inputStream = new StreamReader(path);
				return await MemoryPackSerializer.DeserializeAsync<T>(inputStream.BaseStream);
			}
			catch (IOException) {
				return default;
			}
		}
	}
}