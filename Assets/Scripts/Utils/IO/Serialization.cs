// SnowRainySkr create at 2025-02-17 08:22:39

using System.IO;
using Cysharp.Threading.Tasks;
using MemoryPack;

namespace Arcade.Utils.IO {
	public static class Serialization {
		/// <summary>
		/// Serializes the specified object to a binary file using the MemoryPack library.
		/// This method catches and swallows any <see cref="IOException"/>, so it will not be thrown to the caller.
		/// </summary>
		/// <typeparam name="T">The type of the object to serialize. It must be decorated with the <see cref="MemoryPackableAttribute"/>.</typeparam>
		/// <param name="obj">The object to serialize. It must be of a type decorated with the <see cref="MemoryPackableAttribute"/>.</param>
		/// <param name="path">The file path where the serialized binary data will be saved.</param>
		public static void ToBinFile<T>(this T obj, string path) where T : notnull {
			try {
				File.WriteAllBytes(path, MemoryPackSerializer.Serialize(obj));
			}
			catch (IOException) { }
		}

		/// <summary>
		/// Asynchronously serializes the specified object to a binary file using the MemoryPack library.
		/// This method catches and swallows any <see cref="IOException"/>, so it will not be thrown to the caller.
		/// </summary>
		/// <typeparam name="T">The type of the object to serialize. It must be decorated with the <see cref="MemoryPackableAttribute"/>.</typeparam>
		/// <param name="obj">The object to serialize. It must be decorated with the <see cref="MemoryPackableAttribute"/>.</param>
		/// <param name="path">The file path where the serialized object will be saved.</param>
		public static async UniTask ToBinFileAsync<T>(this T obj, string path) where T : notnull {
			try {
				await using var outputStream = new StreamWriter(path);
				await MemoryPackSerializer.SerializeAsync(outputStream.BaseStream, obj);
			}
			catch (IOException) { }
		}

		/// <summary>
		/// Deserializes a binary file into an object of the specified type using the MemoryPack library.
		/// This method catches any <see cref="IOException"/> that may occur during the file read operation
		/// and returns the default value for the type T in such cases.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize. It must be decorated with the <see cref="MemoryPackableAttribute"/>.</typeparam>
		/// <param name="path">The file path to the binary file that contains the serialized object.</param>
		/// <returns>
		/// The deserialized object of type T, or the default value for T if an <see cref="IOException"/> occurs.
		/// </returns>
		public static T? FromBinFile<T>(this string path) {
			try {
				return MemoryPackSerializer.Deserialize<T>(File.ReadAllBytes(path));
			}
			catch (IOException) {
				return default;
			}
		}

		/// <summary>
		/// Asynchronously deserializes a binary file into an object of the specified type using the MemoryPack library.
		/// This method catches any <see cref="IOException"/> that may occur during the file read operation
		/// and returns the default value for the type T in such cases.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize. It must be decorated with the <see cref="MemoryPackableAttribute"/>.</typeparam>
		/// <param name="path">The file path to the binary file that contains the serialized object.</param>
		/// <returns>
		/// A task that represents the asynchronous deserialization operation. The task result contains the deserialized object of type T,
		/// or the default value for T if an <see cref="IOException"/> occurs.
		/// </returns>
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