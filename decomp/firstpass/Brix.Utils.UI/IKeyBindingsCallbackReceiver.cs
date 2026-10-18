using System;
using System.Collections.Generic;
using Rewired;

namespace Brix.Utils.UI;

public interface IKeyBindingsCallbackReceiver<TEnum> where TEnum : struct
{
	Dictionary<TEnum, string> BindingActionNameMap { get; }

	Dictionary<TEnum, string> BindingCategoryNameMap { get; }

	Dictionary<TEnum, Action> BindingActionMap { get; }

	string[] CategoryNameInclusions { get; }

	string[] CategoryNameExclusions { get; }

	Dictionary<TEnum, Action<InputActionEventData>> BindingInputActionCache { get; set; }

	Dictionary<int, bool> BindingControllerEnabledCache { get; set; }

	bool KeyBindingsConnected { get; set; }

	bool CategoriesActive { get; set; }
}
