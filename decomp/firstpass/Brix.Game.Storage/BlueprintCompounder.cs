using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Storage;

public class BlueprintCompounder : MonoBehaviour
{
	private Signal resetSignals = new Signal();

	[SerializeField]
	private IBlueprintTracker _bluePrintTracker;

	[SerializeField]
	private CompoundRecepteur _compoundRecepteur;

	public IBlueprintTracker BluePrintTracker
	{
		get
		{
			return _bluePrintTracker;
		}
		set
		{
			resetSignals.Invoke();
			_bluePrintTracker = value;
			if (_bluePrintTracker != null && _bluePrintTracker != null)
			{
				_bluePrintTracker.BlueprintAddedSignal.Connect(OnBluePrintAdded).DisconnectWhen(resetSignals);
				_bluePrintTracker.BlueprintRemovedSignal.Connect(OnBluePrintRemoved).DisconnectWhen(resetSignals);
				(from b in _bluePrintTracker.TrackedBlueprints.Slinq()
					select b.recepteur).ForEach(CompoundRecepteur.Add);
			}
		}
	}

	public CompoundRecepteur Recepteur => _compoundRecepteur;

	private CompoundRecepteur CompoundRecepteur
	{
		get
		{
			return _compoundRecepteur;
		}
		set
		{
			if (_compoundRecepteur != null)
			{
				_compoundRecepteur.Reset();
			}
			_compoundRecepteur = value;
		}
	}

	public int QtyNeeded(Adjectif adj)
	{
		return _compoundRecepteur.BaseCapacity.Value(adj) - _compoundRecepteur.ContentDescription.Value(adj);
	}

	private void OnValidate()
	{
		CompoundRecepteur = _compoundRecepteur;
		BluePrintTracker = _bluePrintTracker;
	}

	private void Start()
	{
		CompoundRecepteur = CompoundRecepteur;
		BluePrintTracker = BluePrintTracker;
	}

	private void OnBluePrintAdded(Blueprint blueprint)
	{
		if (_compoundRecepteur != null)
		{
			_compoundRecepteur.Add(blueprint.recepteur);
		}
	}

	private void OnBluePrintRemoved(Blueprint blueprint)
	{
		if (_compoundRecepteur != null)
		{
			_compoundRecepteur.Remove(blueprint.recepteur);
		}
	}
}
