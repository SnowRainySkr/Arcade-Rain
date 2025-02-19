// SnowRainySkr create at 2025-02-19 19:58:42

using UnityEngine;

namespace Arcade.Utils.Unity {
	public static class LineRendererUtility {
		public static void DrawLine(this LineRenderer line, Vector3 from, Vector3 to) {
			line.positionCount = 2;
			line.SetPosition(0, from);
			line.SetPosition(1, to);
		}
	}
}