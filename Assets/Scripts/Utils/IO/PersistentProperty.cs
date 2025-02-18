// SnowRainySkr create at 2025-02-17 20:58:05

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Arcade.Utils.IO {
	using PersistentDataDictionary = Dictionary<string, object?>;

	public static class PersistentProperty {
		/// <summary>
		/// Initializes the PersistentProperty system with the specified local data path.
		/// This method must be called before any other <see cref="PersistentProperty{T}"/> API is used.
		/// </summary>
		/// <param name="localDataPath">The path to the local data directory where the persistent data will be stored.</param>
		public static void Initialize(string localDataPath) {
			IPersistentProperty.InitializeWithLocalDataPath(localDataPath);
		}
	}

	internal interface IPersistentProperty {
		private static readonly PersistentDataDictionary Data = new();

		private static string? _localDataPath;

		internal static void InitializeWithLocalDataPath(string localDataPath) {
			if (_localDataPath is null) {
				throw new InvalidOperationException(
					$"{nameof(PersistentProperty.Initialize)} can only be called once."
				);
			}

			if ((_localDataPath = localDataPath).FromBinFile<PersistentDataDictionary>() is not { } data) return;
			foreach (var (propertyID, value) in data) Data[propertyID] = value;
		}

		private static void CheckIfInitialized() {
			if (_localDataPath is not null) return;
			throw new InvalidOperationException($"Not initialized yet, call {nameof(PersistentProperty.Initialize)}.");
		}

		private static void Save() => Data.ToBinFile(_localDataPath!);

		protected static void InitializeWithDefault<T>(string propertyID, T? defaultValue) {
			Data.TryAdd(propertyID, defaultValue);
		}

		protected static T GetValueByPropertyID<T>(string propertyID) {
			CheckIfInitialized();
			return (T)Data[propertyID]!;
		}

		protected static void SetValueByPropertyID<T>(string propertyID, T newValue) {
			CheckIfInitialized();
			Data[propertyID] = newValue;
			Save();
		}
	}

	/// <summary>
	/// Represents a property that automatically persists its value to local storage whenever it is changed.
	/// All methods of this class, except for the constructor, require prior initialization by calling <see cref="PersistentProperty.Initialize"/>.
	/// After initialization, no exceptions will be thrown by the methods of this class.
	/// </summary>
	/// <typeparam name="T">The type of the value to be stored.
	/// It must be of a type decorated with the <see cref="MemoryPack.MemoryPackableAttribute"/>.</typeparam>
	public readonly struct PersistentProperty<T> : IPersistentProperty {
		private string? PropertyID { get; }

		private void CheckIfInitialized() {
			if (PropertyID is not null) return;
			throw new InvalidOperationException("Invoke the parameterized constructor.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistentProperty{T}"/> class with a default value.
		/// The default value is used when the property does not exist in local storage.
		/// </summary>
		/// <param name="defaultValue">The default value to be used if the property is not found in local storage.</param>
		/// <param name="callMemberName">
		/// The name of the member that invoked this constructor, automatically populated by the compiler.
		/// This is used as the identifier for the property in local storage.
		/// </param>
		public PersistentProperty(T defaultValue, [CallerMemberName] string callMemberName = "") {
			IPersistentProperty.InitializeWithDefault(PropertyID = callMemberName, defaultValue);
		}

		/// <summary>
		/// Gets the current value of the property. This method requires prior initialization.
		/// </summary>
		/// <returns>The current value of the property.</returns>
		public T Value {
			get {
				CheckIfInitialized();
				return IPersistentProperty.GetValueByPropertyID<T>(PropertyID!);
			}
		}

		/// <summary>
		/// Provides an implicit conversion from <see cref="PersistentProperty{T}"/> to <typeparamref name="T"/>.
		/// This method requires prior initialization.
		/// </summary>
		/// <param name="property">The <see cref="PersistentProperty{T}"/> to convert.</param>
		/// <returns>The value of the <see cref="PersistentProperty{T}"/>.</returns>
		public static implicit operator T(in PersistentProperty<T> property) {
			property.CheckIfInitialized();
			return property.Value;
		}

		/// <summary>
		/// Sets the value of the property. This method requires prior initialization.
		/// The new value will be automatically persisted to local storage.
		/// </summary>
		/// <param name="newValue">The new value to set for the property.</param>
		public void Set(T newValue) {
			CheckIfInitialized();
			IPersistentProperty.SetValueByPropertyID(PropertyID!, newValue);
		}
	}
}