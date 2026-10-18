using System;
using System.Collections.Generic;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectionGroup
{
	public SelectionGroupsID Id;

	public List<SelectionType> compatibleTypes;
}
