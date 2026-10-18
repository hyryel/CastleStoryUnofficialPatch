using System;
using System.Collections.Generic;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectionTrack
{
	public SelectionTrackID track;

	public List<SelectionType> types;
}
