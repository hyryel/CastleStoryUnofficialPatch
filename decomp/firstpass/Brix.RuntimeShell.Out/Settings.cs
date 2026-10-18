using System;
using UnityEngine;

namespace Brix.RuntimeShell.Out;

public class Settings : MonoBehaviour
{
	[Serializable]
	public class LanguageBinding
	{
		public string language;

		public string prefix;

		public Color color;
	}

	public LanguageBinding[] languages;

	public Color outputColor;

	public Color errorColor;
}
