using System;
using UnityEngine;

namespace Brix.Game.Components;

public interface IUpdatableComponent
{
	Component Component { get; }

	void BrixUpdate();

	void BrixFixedUpdate();

	void BrixLateUpdate();

	void BrixSlowUpdate();

	void OnUpdateError(Exception e);

	void OnFixedUpdateError(Exception e);

	void OnLateUpdateError(Exception e);

	void OnSlowUpdateError(Exception e);
}
