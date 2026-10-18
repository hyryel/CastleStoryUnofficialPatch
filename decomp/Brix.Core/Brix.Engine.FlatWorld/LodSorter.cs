using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Geometrie;
using Brix.External.Signals;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class LodSorter
{
	public class Child
	{
		public Profile profile;

		public GameObject gameObject;

		public MegaVoxelGeo mv;

		public List<Child> immediateChildren = new List<Child>();

		public Child immediateParent;

		public List<Child> immediateNeighbors = new List<Child>();

		public XYZ start;

		public MegaVoxelGeo MegaVoxel => (mv != null) ? mv : (mv = gameObject.GetComponent<MegaVoxelGeo>());
	}

	public class Profile
	{
		public Transform renderParent;

		public int lod;

		public XYZ start;

		public XYZ unitSize;

		public XYZ maxSize;

		public float shaderOffset;

		public FlatWorld initialWorld;

		public bool selfCreated;

		public Dictionary<XYZ, Child> childObjects = new Dictionary<XYZ, Child>();

		public int exCount = 0;

		public MegaVoxelGeo[] mvs;

		public List<Child> children = new List<Child>();

		private static readonly Dictionary<XYZ, List<Child>> KinshipDico = new Dictionary<XYZ, List<Child>>();

		public void Refresh()
		{
			if (mvs == null || exCount != childObjects.Count)
			{
				exCount = childObjects.Count;
				IEnumerable<MegaVoxelGeo> source = from g in childObjects.Values
					where g != null
					let m = g.mv = g.gameObject.GetComponent<MegaVoxelGeo>()
					where m != null
					select m;
				mvs = source.ToArray();
			}
		}

		public void Init()
		{
			CreateProfileMaterial();
			initialWorld = new FlatWorld();
		}

		public void Destroy()
		{
			childObjects.Clear();
			exCount = 0;
			mvs = null;
			children.Clear();
			DestroyProfileMaterial();
			initialWorld.Destroy();
			if (selfCreated)
			{
				if (renderParent != null)
				{
					Object.Destroy(renderParent.gameObject);
				}
				renderParent = null;
				selfCreated = false;
			}
			else
			{
				for (int num = renderParent.childCount - 1; num >= 0; num--)
				{
					Object.Destroy(renderParent.GetChild(num).gameObject);
				}
			}
		}

		private void CreateProfileMaterial()
		{
			MeshRenderer component = renderParent.GetComponent<MeshRenderer>();
			Material material = new Material(component.sharedMaterial);
			material.SetFloat("_Offset", shaderOffset);
			Material material2 = material;
			material2.name = material2.name + " (Lod " + lod + ")";
			component.sharedMaterial = material;
		}

		private void DestroyProfileMaterial()
		{
			Object.Destroy(renderParent.GetComponent<MeshRenderer>().sharedMaterial);
		}

		public XYZ ComputePositionInParentProfile(XYZ p)
		{
			return XYZ.Mul(XYZ.Div(p, unitSize) >> 1 << 1, unitSize);
		}

		public void RefreshKinship(Profile parentProfile)
		{
			if (parentProfile == null)
			{
				return;
			}
			KinshipDico.Clear();
			Dictionary<XYZ, Child>.Enumerator enumerator = childObjects.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<XYZ, Child> current = enumerator.Current;
				KinshipDico.GetValueOrCreate(ComputePositionInParentProfile(current.Value.start)).Add(current.Value);
			}
			Dictionary<XYZ, List<Child>>.Enumerator enumerator2 = KinshipDico.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<XYZ, List<Child>> current2 = enumerator2.Current;
				Child child = parentProfile.childObjects[current2.Key];
				child.immediateChildren = current2.Value;
				List<Child> value = current2.Value;
				for (int i = 0; i < value.Count; i++)
				{
					Child child2 = value[i];
					child2.immediateParent = child;
				}
			}
		}

		public void RefreshNeighbors()
		{
			for (int i = 0; i < children.Count; i++)
			{
				Child child = children[i];
				XYZ p = child.start;
				IEnumerable<XYZ> source = CubeModele.XYZDirs.Select((XYZ d) => p + d);
				IEnumerable<Child> source2 = source.Select((XYZ q) => childObjects.GetValueOrDefault(q));
				child.immediateNeighbors = source2.ToList();
			}
		}

		public void RegisterChild(XYZ p, GameObject childObject)
		{
			Child child = new Child
			{
				profile = this,
				gameObject = childObject,
				start = p
			};
			childObjects[p] = child;
			children.Add(child);
		}
	}

	private Profile[] profiles = new Profile[10];

	public int lodMax = -1;

	public Signal<GameObject, XYZ, int> RegisterChildSignal = new Signal<GameObject, XYZ, int>();

	public void CreateProfile(Transform renderParent, int lod, XYZ mvSize, float shaderOffset)
	{
		bool selfCreated = false;
		XYZ unitSize = mvSize << lod;
		renderParent = Display.GetChildObject(renderParent, "Lod " + lod, delegate
		{
			selfCreated = true;
		});
		profiles[lod] = new Profile
		{
			renderParent = renderParent,
			lod = lod,
			unitSize = unitSize,
			selfCreated = selfCreated,
			shaderOffset = shaderOffset
		};
		profiles[lod].Init();
		lodMax = Mathf.Max(lodMax, lod);
	}

	public bool HasProfile(int lod)
	{
		Profile profile = profiles[lod];
		return profile != null && profile.renderParent != null;
	}

	public void DestroyProfile(int lod)
	{
		if (profiles[lod] != null)
		{
			profiles[lod].Destroy();
			profiles[lod] = null;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < profiles.Length; i++)
		{
			DestroyProfile(i);
		}
		lodMax = -1;
		RegisterChildSignal.Clear();
	}

	public SlinqEnumerable<Profile, PredicateContext<Profile, IListContext<Profile>>, Profile[]> GetActiveProfiles()
	{
		return SlinqEnumerable.Make((Profile[] _profiles) => from p in _profiles.Slinq()
			where p != null
			select p, profiles);
	}

	public XYZ GetProfileUnitSize(int lod)
	{
		return profiles[lod].unitSize;
	}

	public Transform GetLodRenderParent(int lod)
	{
		return profiles[lod]?.renderParent;
	}

	public FlatWorld GetLodInitialWorld(int lod)
	{
		return profiles[lod].initialWorld;
	}

	public void RegisterChildObject(GameObject childObject, XYZ p, int lod)
	{
		profiles[lod].RegisterChild(p, childObject);
		RegisterChildSignal.Invoke(childObject, p, lod);
	}

	public void RefreshKinship(int lod)
	{
		profiles[lod].RefreshKinship(profiles[lod + 1]);
	}

	public void RefreshNeighbors(int lod)
	{
		profiles[lod].RefreshNeighbors();
	}

	public Child GetLodChild(XYZ p, int lod)
	{
		Profile profile = profiles[lod];
		return profile.childObjects.GetValueOrDefault(p);
	}

	public List<Child> GetLodChildren(int lod)
	{
		Profile profile = profiles[lod];
		return profile.children;
	}

	public GameObject GetLodChildObject(XYZ p, int lod)
	{
		return GetLodChild(p, lod)?.gameObject;
	}

	public IEnumerable<GameObject> GetLodChildObjects(int lod)
	{
		Profile profile = profiles[lod];
		return profile.childObjects.Values.Select((Child child) => child.gameObject);
	}

	public MegaVoxelGeo[] GetLodMegaVoxels(int lod)
	{
		Profile profile = profiles[lod];
		profile.Refresh();
		return profile.mvs;
	}

	public IEnumerable<XYZ> GetLodChildPositions(int lod)
	{
		Profile profile = profiles[lod];
		return profile.childObjects.Values.Select((Child child) => child.start);
	}
}
