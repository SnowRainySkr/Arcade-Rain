// SnowRainySkr create at 2025-02-17 08:58:05

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Arcade.Utils.IO {
	using PersistentDataDictionary = Dictionary<string, object?>;

	public static class PersistentProperty {
		public static void Initialize(string localDataPath) {
			IPersistentProperty.InitializeWithLocalDataPath(localDataPath);
		}
	}

	internal interface IPersistentProperty {
		private static readonly PersistentDataDictionary Data = new();

		private static string? _localDataPath;

		internal static void InitializeWithLocalDataPath(string localDataPath) {
			if (_localDataPath is null) {
				throw new InvalidOperationException($"{nameof(PersistentProperty.Initialize)} can only be called once.");
			}

			if ((_localDataPath = localDataPath).FromBinFile<PersistentDataDictionary>() is not { } data) return;
			foreach (var (propertyID, value) in data) Data[propertyID] = value;
		}

		private static void CheckIfInitialized() {
			if (_localDataPath is null) {
				throw new InvalidOperationException($"Not initialized yet, call {nameof(PersistentProperty.Initialize)}.");
			}
		}

		private static void Save() => Data.ToBinFile(_localDataPath!);

		protected static void InitializeWithDefault<T>(string propertyID, T defaultValue) {
			CheckIfInitialized();
			if (Data.TryAdd(propertyID, defaultValue)) Save();
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

	public readonly struct PersistentProperty<T> : IPersistentProperty {
		private string PropertyID { get; }

		public PersistentProperty(T defaultValue, [CallerMemberName] string callMemberName = "") {
			IPersistentProperty.InitializeWithDefault(PropertyID = callMemberName, defaultValue);
		}

		public T Value => IPersistentProperty.GetValueByPropertyID<T>(PropertyID);

		public static implicit operator T(in PersistentProperty<T> property) => property.Value;

		public void Set(T newValue) => IPersistentProperty.SetValueByPropertyID(PropertyID, newValue);
	}
}