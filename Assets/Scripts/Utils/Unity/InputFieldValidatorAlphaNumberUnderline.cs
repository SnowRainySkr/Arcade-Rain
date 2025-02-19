// SnowRainySkr create at 2025-02-19 19:01:27

using TMPro;
using UnityEngine;

namespace Arcade.Utils.Unity {
	[CreateAssetMenu]
	public sealed class InputFieldValidatorAlphaNumberUnderline : TMP_InputValidator {
		public override char Validate(ref string text, ref int pos, char ch) {
			if (pos++ >= 36 || ch is not (>= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_')) {
				return '\0';
			}

			text = text.Insert(pos - 1, ch.ToString());
			return ch;
		}
	}
}