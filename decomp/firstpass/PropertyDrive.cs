using System.Collections.Generic;
using System.Linq;
using Brix.Common.Collections;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PropertyDrive : MonoBehaviour, ISerializationCallbackReceiver
{
	private static BrixLogger logger = new BrixLogger(typeof(PropertyDrive).FullName);

	public static bool isDebug = false;

	public bool RecastNextFrame;

	public Component traverseDriver;

	public float traverse;

	public bool needsUpdate = true;

	public bool autoUpdates;

	public UnityEvent m_OnApply;

	public int propertyBlendCount;

	public PropertyBlend[] propertyBlends = new PropertyBlend[0];

	public Comp[] comps = new Comp[0];

	public float Traverse
	{
		get
		{
			return traverse;
		}
		set
		{
			if (traverse != value)
			{
				needsUpdate = true;
			}
			traverse = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public PropertyBlend[] PropertyBlends
	{
		get
		{
			List<PropertyBlend> list = propertyBlends.Where((PropertyBlend propertyBlend) => propertyBlend != null).ToList();
			list.Add(null);
			return list.ToArray();
		}
	}

	public Comp[] Comps => comps;

	public void Apply()
	{
		if (isDebug)
		{
			logger.Info(Mathf.Round(Time.time * 100f) / 100f + " :: " + base.name + "[PropertyDrive].Apply(){ needsUpdate == " + needsUpdate + " }\n");
		}
		if (!needsUpdate)
		{
			return;
		}
		needsUpdate = false;
		foreach (Comp item in (from c in Comps.Slinq()
			where c.component != null && c.props != null && c.propsIdx < c.props.Length
			select c).NoLeak())
		{
			Prop prop = item.props[item.propsIdx];
			PropertyBlend propertyBlend = item.propertyBlend;
			if (prop != null && propertyBlend != null && prop.Matches(propertyBlend))
			{
				if (prop is FloatProp floatProp)
				{
					FloatPropertyBlend floatPropertyBlend = propertyBlend as FloatPropertyBlend;
					floatProp.sets(floatPropertyBlend.Apply(Traverse));
				}
				if (prop is Vector3Prop vector3Prop)
				{
					Vector3PropertyBlend vector3PropertyBlend = propertyBlend as Vector3PropertyBlend;
					vector3Prop.sets(vector3PropertyBlend.Apply(Traverse));
				}
				if (prop is QuaternionProp quaternionProp)
				{
					QuaternionPropertyBlend quaternionPropertyBlend = propertyBlend as QuaternionPropertyBlend;
					quaternionProp.sets(quaternionPropertyBlend.Apply(Traverse));
				}
				if (prop is ColorProp colorProp)
				{
					ColorPropertyBlend colorPropertyBlend = propertyBlend as ColorPropertyBlend;
					colorProp.sets(colorPropertyBlend.Apply(Traverse));
				}
			}
		}
		m_OnApply.Invoke();
	}

	private void Sanitize<T>(ref T[] ts) where T : class
	{
		if (ts == null)
		{
			ts = new T[0];
		}
		List<T> list = ts.Where((T t) => t != null).ToList();
		list.Add((T)null);
		ts = list.ToArray();
	}

	private void Start()
	{
		RecastOld();
	}

	private void OnEnable()
	{
		RecastOld();
		needsUpdate = true;
	}

	private void Awake()
	{
		RecastOld();
	}

	[ContextMenu("RecastOld")]
	public void RecastOld()
	{
	}

	[ContextMenu("Recast")]
	public void Recast()
	{
		List<Comp> list = new List<Comp>();
		foreach (Comp item in comps.Where((Comp comp) => comp.component != null))
		{
			if (item.component as Light != null)
			{
				list.Add(new LightComp(item));
			}
			else if (item.component as Image != null)
			{
				list.Add(new ImageComp(item));
			}
			else if (item.component as Text != null)
			{
				list.Add(new TextComp(item));
			}
			else if (item.component as LayoutElement != null)
			{
				list.Add(new LayoutElementComp(item));
			}
			else if (item.component as HorizontalLayoutGroup != null)
			{
				list.Add(new HorizontalLayoutGroupComp(item));
			}
			else if (item.component as VerticalLayoutGroup != null)
			{
				list.Add(new VerticalLayoutGroupComp(item));
			}
			else if (item.component as RadialLayoutElementCore != null)
			{
				list.Add(new RadialLayoutElementCoreComp(item));
			}
			else if (item.component as RadialLayoutElement != null)
			{
				list.Add(new RadialLayoutElementComp(item));
			}
			else if (item.component as RadialLayoutGroup != null)
			{
				list.Add(new RadialLayoutGroupComp(item));
			}
			else if (item.component as Transform != null)
			{
				list.Add(new TransformComp(item));
			}
			else if (item.component as MeshRenderer != null)
			{
				list.Add(new MeshRendererComp(item));
			}
		}
		list.Add(new Comp());
		comps = list.ToArray();
	}

	private void Update()
	{
		if (RecastNextFrame)
		{
			RecastNextFrame = false;
			Recast();
		}
		Apply();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (Threading.IsOnMainThread)
		{
			Recast();
		}
		else
		{
			RecastNextFrame = true;
		}
	}
}
