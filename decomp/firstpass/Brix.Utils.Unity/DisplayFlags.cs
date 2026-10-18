using System;

namespace Brix.Utils.Unity;

[Flags]
public enum DisplayFlags
{
	HideAtRuntime = 1,
	HideInEditMode = 2,
	Hide = 3,
	ReadOnlyAtRuntime = 4,
	ReadOnlyInEditMode = 8,
	ReadOnly = 0xC
}
