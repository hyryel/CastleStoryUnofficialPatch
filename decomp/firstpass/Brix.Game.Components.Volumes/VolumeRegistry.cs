using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game.Components.Volumes.Requests;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public static class VolumeRegistry
{
	public static VolumeRegistryData _data = new VolumeRegistryData();

	public static List<HashSet<XYZ>> LowResBlueprints = new List<HashSet<XYZ>>();

	private static VPropertyMask RestrictingDropVProperties = VPropertyMask.occupy | VPropertyMask.operation;

	private static Vector3[] AjustementPositions = new Vector3[4]
	{
		Vector3.forward,
		Vector3.right,
		Vector3.left,
		Vector3.back
	};

	public static SinglePropertyRequest VolumesWithPropertyAt(XYZ position, VProperty p)
	{
		return new SinglePropertyRequest(position, p);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p)
	{
		return VolumesWithPropertyAt(positions, p, (IVolume volume) => true);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAt(IList<XYZ> positions, VProperty p)
	{
		return VolumesWithPropertyAt(positions, p, (IVolume volume) => true);
	}

	public static SinglePropertyFilteredRequest VolumesWithPropertyAt(XYZ position, VProperty p, Func<IVolume, bool> test)
	{
		return new SinglePropertyFilteredRequest(position, p, test);
	}

	public static SinglePropertyFilteredRequest<TArgs> VolumesWithPropertyAt<TArgs>(XYZ position, VProperty p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		return new SinglePropertyFilteredRequest<TArgs>(position, p, test, args);
	}

	public static SinglePropertyClosedFilteredRequest<TFunc> VolumesWithPropertyAt<TFunc>(XYZ position, VProperty p, TFunc test) where TFunc : IFunc<IVolume, bool>
	{
		return new SinglePropertyClosedFilteredRequest<TFunc>(position, p, test);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p, Func<IVolume, bool> test)
	{
		return VolumesWithPropertyAt(positions, p.ToMask(), test);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAt(IList<XYZ> positions, VProperty p, Func<IVolume, bool> test)
	{
		return VolumesWithPropertyAt(positions, p.ToMask(), test);
	}

	public static MultiPropertyRequest VolumesWithPropertyAt(XYZ position, VPropertyMask p)
	{
		return VolumesWithPropertyAt(position, p, (IVolume volume) => true);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p)
	{
		return VolumesWithPropertyAt(positions, p, (IVolume volume) => true);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAt(IList<XYZ> positions, VPropertyMask p)
	{
		return VolumesWithPropertyAt(positions, p, (IVolume volume) => true);
	}

	public static MultiPropertyRequest VolumesWithPropertyAt(XYZ position, VPropertyMask p, Func<IVolume, bool> test)
	{
		return new MultiPropertyRequest(position, p, test);
	}

	public static MultiPropertyRequest<TArgs> VolumesWithPropertyAt<TArgs>(XYZ position, VPropertyMask p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		return new MultiPropertyRequest<TArgs>(position, p, test, args);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return new MultiPropertyMultiVoxelRequest(positions, p, test);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAt(IList<XYZ> positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return new MultiPropertyListVoxelRequest(positions, p, test);
	}

	public static MultiPropertyRequest VolumesWithPropertyAtExcluding(XYZ position, VProperty p, IVolume excluded)
	{
		return VolumesWithPropertyAtExcluding(position, p.ToMask(), excluded);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VProperty p, IVolume excluded)
	{
		return VolumesWithPropertyAtExcluding(positions, p.ToMask(), excluded);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAtExcluding(IList<XYZ> positions, VProperty p, IVolume excluded)
	{
		return VolumesWithPropertyAtExcluding(positions, p.ToMask(), excluded);
	}

	public static MultiPropertyRequest VolumesWithPropertyAtExcluding(XYZ position, VPropertyMask p, IVolume excluded)
	{
		return new MultiPropertyRequest(position, p, (IVolume volume) => true, excluded);
	}

	public static MultiPropertyMultiVoxelRequest VolumesWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VPropertyMask p, IVolume excluded)
	{
		return new MultiPropertyMultiVoxelRequest(positions, p, (IVolume volume) => true, excluded);
	}

	public static MultiPropertyListVoxelRequest VolumesWithPropertyAtExcluding(IList<XYZ> positions, VPropertyMask p, IVolume excluded)
	{
		return new MultiPropertyListVoxelRequest(positions, p, (IVolume volume) => true, excluded);
	}

	public static bool HasVolumesWithPropertyAt(XYZ position, VProperty p)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.Has(p);
	}

	public static bool HasVolumesWithPropertyAt(XYZ position, VPropertyMask p)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasAny(p);
	}

	public static bool HasVolumesWithPropertyAt(XYZ position, VProperty p, Func<IVolume, bool> test)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test);
	}

	public static bool HasVolumesWithPropertyAt<TArgs>(XYZ position, VProperty p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test, args);
	}

	public static bool HasVolumesWithPropertyAt<TFunc>(XYZ position, VProperty p, TFunc test) where TFunc : IFunc<IVolume, bool>
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test);
	}

	public static bool HasVolumesWithPropertyAt(XYZ position, VPropertyMask p, Func<IVolume, bool> test)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test);
	}

	public static bool HasVolumesWithPropertyAt<TArgs>(XYZ position, VPropertyMask p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test, args);
	}

	public static bool HasVolumesWithPropertyAt<TFunc>(XYZ position, VPropertyMask p, TFunc test) where TFunc : IFunc<IVolume, bool>
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, test);
	}

	public static bool HasVolumesWithPropertyAtExcluding(XYZ position, VProperty p, IVolume excluded)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, (IVolume v, IVolume e) => v != e, excluded);
	}

	public static bool HasVolumesWithPropertyAtExcluding(XYZ position, VPropertyMask p, IVolume excluded)
	{
		RegistryEntry value;
		return _data.IndexedEntries.TryGetValue(position, out value) && value.HasVolumesWith(p, (IVolume v, IVolume e) => v != e, excluded);
	}

	public static bool HasVolumesWithPropertyAt(IList<XYZ> positions, VProperty p)
	{
		return positions.Slinq().Any(HasVolumesWithPropertyAt, p);
	}

	public static bool HasVolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p)
	{
		return positions.Slinq().Any(HasVolumesWithPropertyAt, p);
	}

	public static bool HasVolumesWithPropertyAt(IList<XYZ> positions, VPropertyMask p)
	{
		return positions.Slinq().Any(HasVolumesWithPropertyAt, p);
	}

	public static bool HasVolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p)
	{
		return positions.Slinq().Any(HasVolumesWithPropertyAt, p);
	}

	public static bool HasVolumesWithPropertyAt(IList<XYZ> positions, VProperty p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VProperty, Func<IVolume, bool>> t) => HasVolumesWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test));
	}

	public static bool HasVolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VProperty, Func<IVolume, bool>> t) => HasVolumesWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test));
	}

	public static bool HasVolumesWithPropertyAt(IList<XYZ> positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VPropertyMask, Func<IVolume, bool>> t) => HasVolumesWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test));
	}

	public static bool HasVolumesWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VPropertyMask, Func<IVolume, bool>> t) => HasVolumesWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test));
	}

	public static bool HasVolumesWithPropertyAtExcluding(IList<XYZ> positions, VProperty p, IVolume excluded)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VProperty, IVolume> t) => HasVolumesWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded));
	}

	public static bool HasVolumesWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VProperty p, IVolume excluded)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VProperty, IVolume> t) => HasVolumesWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded));
	}

	public static bool HasVolumesWithPropertyAtExcluding(IList<XYZ> positions, VPropertyMask p, IVolume excluded)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VPropertyMask, IVolume> t) => HasVolumesWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded));
	}

	public static bool HasVolumesWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VPropertyMask p, IVolume excluded)
	{
		return positions.Slinq().Any((XYZ position, Tuple<VPropertyMask, IVolume> t) => HasVolumesWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded));
	}

	public static IVolume VolumeWithPropertyAt(XYZ position, VProperty p)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p);
	}

	public static IVolume VolumeWithPropertyAt(XYZ position, VPropertyMask p)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p);
	}

	public static IVolume VolumeWithPropertyAt(XYZ position, VProperty p, Func<IVolume, bool> test)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, test);
	}

	public static IVolume VolumeWithPropertyAt<TArgs>(XYZ position, VProperty p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, test, args);
	}

	public static IVolume VolumeWithPropertyAt(XYZ position, VPropertyMask p, Func<IVolume, bool> test)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, test);
	}

	public static IVolume VolumeWithPropertyAt<TArgs>(XYZ position, VPropertyMask p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, test, args);
	}

	public static IVolume VolumeWithPropertyAtExcluding(XYZ position, VProperty p, IVolume excluded)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, (IVolume v, IVolume e) => v != e, excluded);
	}

	public static IVolume VolumeWithPropertyAtExcluding(XYZ position, VPropertyMask p, IVolume excluded)
	{
		RegistryEntry value;
		return (!_data.IndexedEntries.TryGetValue(position, out value)) ? null : value.GetVolumeWith(p, (IVolume v, IVolume e) => v != e, excluded);
	}

	public static IVolume VolumeWithPropertyAt(IList<XYZ> positions, VProperty p)
	{
		return positions.Slinq().Select(VolumeWithPropertyAt, p).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p)
	{
		return positions.Slinq().Select(VolumeWithPropertyAt, p).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(IList<XYZ> positions, VPropertyMask p)
	{
		return positions.Slinq().Select(VolumeWithPropertyAt, p).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p)
	{
		return positions.Slinq().Select(VolumeWithPropertyAt, p).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(IList<XYZ> positions, VProperty p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VProperty, Func<IVolume, bool>> t) => VolumeWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(VolumeMultiPropertyRequest positions, VProperty p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VProperty, Func<IVolume, bool>> t) => VolumeWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(IList<XYZ> positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VPropertyMask, Func<IVolume, bool>> t) => VolumeWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAt(VolumeMultiPropertyRequest positions, VPropertyMask p, Func<IVolume, bool> test)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VPropertyMask, Func<IVolume, bool>> t) => VolumeWithPropertyAt(position, t._1, t._2), Tuple.Create(p, test)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAtExcluding(IList<XYZ> positions, VProperty p, IVolume excluded)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VProperty, IVolume> t) => VolumeWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VProperty p, IVolume excluded)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VProperty, IVolume> t) => VolumeWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAtExcluding(IList<XYZ> positions, VPropertyMask p, IVolume excluded)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VPropertyMask, IVolume> t) => VolumeWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded)).NotNull()
			.FirstOrDefault();
	}

	public static IVolume VolumeWithPropertyAtExcluding(VolumeMultiPropertyRequest positions, VPropertyMask p, IVolume excluded)
	{
		return positions.Slinq().Select((XYZ position, Tuple<VPropertyMask, IVolume> t) => VolumeWithPropertyAtExcluding(position, t._1, t._2), Tuple.Create(p, excluded)).NotNull()
			.FirstOrDefault();
	}

	public static bool HasVolumesWithPropertyAtLod(XYZ lodResIndex, VProperty p, int lod)
	{
		if (p != VProperty.fantome && p != VProperty.access)
		{
			return false;
		}
		return lod >= 0 && lod < LowResBlueprints.Count && LowResBlueprints[lod].Contains(lodResIndex);
	}

	public static IEnumerable<IVolume> GetAllRegisteredObjects()
	{
		Slinq<IVolume, SelectContext<IVolume, KeyValuePair<IVolume, BaseVolume>, DictionaryContext<IVolume, BaseVolume>>> volumesEnumerator = from v in _data.RegisteredVolumesData.Slinq()
			select v.Key;
		while (volumesEnumerator.current.isSome)
		{
			yield return volumesEnumerator.current.value;
			volumesEnumerator.Skip();
		}
	}

	public static IEnumerable<T> GetAllRegisteredObjects<T>() where T : class
	{
		Slinq<IVolume, PredicateContext<IVolume, SelectContext<IVolume, KeyValuePair<IVolume, BaseVolume>, DictionaryContext<IVolume, BaseVolume>>>> volumesEnumerator = from v in _data.RegisteredVolumesData.Slinq()
			select v.Key into k
			where !(k is FixedVolume)
			select k;
		while (volumesEnumerator.current.isSome)
		{
			if (volumesEnumerator.current.value is T value)
			{
				yield return value;
			}
			volumesEnumerator.Skip();
		}
	}

	public static Vector3 ClosestAvailableDropPosition(Transform reference, int objectCount, GameObject dropSubjectSample, bool forceDrop = false)
	{
		Vector3[] ajustementPositions = AjustementPositions;
		foreach (Vector3 position in ajustementPositions)
		{
			Vector3 vector = reference.TransformPoint(position);
			if (objectCount == 1)
			{
				vector += new Vector3(0f, 0.5f, 0f);
			}
			else
			{
				vector += Vector3.up;
			}
			if (!HasVolumesWithPropertyAt(XYZ.FromVector3(vector), RestrictingDropVProperties) && (forceDrop || objectCount != 1 || DropValidAtPosition(dropSubjectSample, XYZ.FromVector3(vector), PreprocessedRotations.RotationToDiscreteIndex(reference.rotation))))
			{
				return vector;
			}
		}
		return Vector3.zero;
	}

	public static bool DropValidAtPosition(GameObject dropSubjectSample, XYZ position, int rotationIndex)
	{
		if (!dropSubjectSample.IsNullOrReleased())
		{
			ValidatorComponent component = dropSubjectSample.GetComponent<ValidatorComponent>();
			if (!component.IsNullOrReleased())
			{
				FreeVolume freeVolume = FreeVolume.Make(component.AssetKey, position, rotationIndex);
				ValidationData vData = freeVolume.VData;
				ValidationResult validationResult = component.Validator.Validate(ref vData);
				freeVolume.Dispose();
				if (validationResult != ValidationResult.Valid)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static IEnumerable<T> ComponentsFromVolumeAt<T>(XYZ index, VProperty property, Func<GameObject, bool> condition = null)
	{
		if ((Delegate)condition == (Delegate)null)
		{
			condition = (GameObject go) => true;
		}
		Slinq<GameObject, PredicateContext<GameObject, NotNullContext<GameObject, SelectContext<GameObject, IVolume, IListContext<IVolume>>>, Func<GameObject, bool>>> items = (from v in VolumesWithPropertyAt(index, property).Slinq()
			select v?.GameObject).NotNull().Where((GameObject v, Func<GameObject, bool> c) => c(v), condition);
		foreach (GameObject i in items.NoLeak())
		{
			T component = i.GetComponent<T>();
			if (component != null)
			{
				yield return component;
			}
		}
	}
}
