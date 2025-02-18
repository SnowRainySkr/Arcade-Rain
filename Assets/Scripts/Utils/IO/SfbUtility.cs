// SnowRainySkr create at 2025-02-18 11:48:35

using System.Collections.Generic;
using System.Linq;
using SFBr = SFB.StandaloneFileBrowser;

namespace Arcade.Utils.IO {
	public static class SFBUtility {
		public static string? SaveFile(
			string defaultSaveFileName, string extension, string title = "选择保存文件的路径", string defaultPath = ""
		) {
			var sfbSelectResult = SFBr.SaveFilePanel(title, defaultPath, defaultSaveFileName, extension);
			return sfbSelectResult is "" ? null : sfbSelectResult;
		}

		public static string? SelectSingleFile(string extension, string title = "选择文件", string defaultPath = "") {
			return SFBr.OpenFilePanel(title, defaultPath, extension, false).FirstOrDefault();
		}

		public static List<string> SelectMultipleFiles(string extension, string title = "选择文件", string defaultPath = "") {
			return SFBr.OpenFilePanel(title, defaultPath, extension, true).ToList();
		}

		public static string? SelectSingleFolder(string title = "选择文件夹", string defaultPath = "") {
			return SFBr.OpenFolderPanel(title, defaultPath, false).FirstOrDefault();
		}

		public static List<string> SelectMultipleFolders(string title = "选择文件夹", string defaultPath = "") {
			return SFBr.OpenFolderPanel(title, defaultPath, true).ToList();
		}
	}
}