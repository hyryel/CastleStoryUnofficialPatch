using System.Collections.Generic;
using Brix.Input;

namespace Brix.Game.Components;

public interface IDragger
{
	bool AutoOrient { get; }

	void SetDragStart(VoxelRaycastHit vrh, PickerComponent pc);

	IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh);

	IEnumerable<Slave> GetSlaves();

	void ClearSlaves();
}
