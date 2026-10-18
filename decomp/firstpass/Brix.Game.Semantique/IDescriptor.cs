using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.Semantique;

public interface IDescriptor
{
	Description Description { get; }

	GameObject GameObject { get; }

	IGameComponent GameComponent { get; }

	Factory.AssetKey AssetKey { get; }

	Ressource Principale { get; }
}
