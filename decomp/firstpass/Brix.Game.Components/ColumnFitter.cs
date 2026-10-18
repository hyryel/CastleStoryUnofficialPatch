using Brix.External.Factories;
using Brix.Input;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

internal class ColumnFitter : AbstractFitter
{
	public enum ColumnKeyState
	{
		Base,
		Middle,
		Top
	}

	private int _keyState;

	private Factory.AssetKey[] _selectedKeyCycle = _keyCycle1;

	private static Factory.AssetKey[] _keyCycle1 = new Factory.AssetKey[3]
	{
		Blueprints.Stone_Column_Base,
		Blueprints.Stone_Column_Middle,
		Blueprints.Stone_Column_Top
	};

	private static Factory.AssetKey[] _keyCycle2 = new Factory.AssetKey[3]
	{
		Blueprints.Stone_Column2_Base,
		Blueprints.Stone_Column2_Middle,
		Blueprints.Stone_Column2_Top
	};

	private static Factory.AssetKey[] _keyCycle3 = new Factory.AssetKey[3]
	{
		Blueprints.Stone_Column3_Base,
		Blueprints.Stone_Column3_Middle,
		Blueprints.Stone_Column3_Top
	};

	private static Factory.AssetKey[] _keyCycle4 = new Factory.AssetKey[3]
	{
		Blueprints.Stone_Column4_Base,
		Blueprints.Stone_Column4_Middle,
		Blueprints.Stone_Column4_Top
	};

	private static Factory.AssetKey[] _keyCycle5 = new Factory.AssetKey[3]
	{
		Blueprints.Stone_Column5_Base,
		Blueprints.Stone_Column5_Middle,
		Blueprints.Stone_Column5_Top
	};

	private static Factory.AssetKey[][] _keyCycles = new Factory.AssetKey[5][] { _keyCycle1, _keyCycle2, _keyCycle3, _keyCycle4, _keyCycle5 };

	public override void ChangeFit()
	{
		if (_keyState >= 2)
		{
			ColumnDragger component = GetComponent<ColumnDragger>();
			if (component != null && component.GetSlaves().Slinq().Any())
			{
				_keyState = 1;
			}
			else
			{
				_keyState = 0;
			}
		}
		else
		{
			_keyState++;
		}
		SetAs(_selectedKeyCycle[_keyState]);
	}

	public void SetFit(ColumnKeyState state)
	{
		_keyState = (int)state;
		SetAs(_selectedKeyCycle[_keyState]);
	}

	private void SetAs(Factory.AssetKey key)
	{
		if (GetComponent<Picker>() == null)
		{
			GameObject gameObject = ((!(key == null)) ? Factory.Peek(key) : Factory.Peek(Blueprints.Stone_Column_Base));
			GetComponent<VoxelTransform>().InitialCenterOffset = gameObject.GetComponent<VoxelTransform>().InitialCenterOffset;
			GetComponent<MeshFilter>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			GetComponent<BoxCollider>().size = gameObject.GetComponent<BoxCollider>().size;
			GetComponent<FactoryImprint>().AssetKey = key;
		}
	}

	private void SelectKeyCycle(Factory.AssetKey key)
	{
		for (int i = 0; i < _keyCycles.Length; i++)
		{
			if (_keyCycles[i].Slinq().Contains(key))
			{
				_selectedKeyCycle = _keyCycles[i];
			}
		}
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		ColumnFitter columnFitter = original as ColumnFitter;
		if (columnFitter != null)
		{
			_keyState = columnFitter._keyState;
			_selectedKeyCycle = columnFitter._selectedKeyCycle;
			SetAs(_selectedKeyCycle[_keyState]);
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		SelectKeyCycle(GetComponent<AssetKeyHolder>().AssetKey);
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		ColumnFitter columnFitter = go.AddComponent<ColumnFitter>();
		columnFitter.CloneInternals(this);
		return columnFitter;
	}
}
