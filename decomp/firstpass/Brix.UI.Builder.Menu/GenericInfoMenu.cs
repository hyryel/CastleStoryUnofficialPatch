using System;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class GenericInfoMenu : BaseInfoMenu
{
	private GenericInfoMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}
}
