using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Engine;

public interface IClosestNonEmptyVoxelEnumerator : IEnumerator<XYZ>, IDisposable, IEnumerator
{
	XYZ Position { get; set; }

	int CloseDistance { get; set; }

	int MaxDistance { get; set; }
}
