using System.Collections;
using System.Collections.Generic;

namespace Brix.Engine;

public interface IPartialVoxelVolume : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	IEnumerable<XYZ> FullVoxels { get; }

	IEnumerable<XYZ> PartialVoxels { get; }

	bool FullyContains(XYZ p);

	bool PartiallyContains(XYZ p);
}
