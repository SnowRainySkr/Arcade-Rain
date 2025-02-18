// SnowRainySkr create at 2025-02-18 13:37:36

using UnityEngine;

namespace Arcade.Utils.IO {
	public static class FileExplorerHelper {
		public static void OpenFileInExplorer(this string path) => Application.OpenURL("file://" + path);
	}
}