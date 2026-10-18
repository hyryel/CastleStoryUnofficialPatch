using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class CompassDispenser : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(CompassDispenser).FullName);

	public static bool debug = false;

	public static string debugName = "Crystal";

	[SerializeField]
	private GameObject m_SoloCompassCloneable;

	[SerializeField]
	private GameObject m_GroupCompassCloneable;

	[SerializeField]
	private GameObject m_SoloCompassCloneBin;

	[SerializeField]
	private GameObject m_GroupCompassCloneBin;

	private List<SoloCompass> m_SoloCompasses = new List<SoloCompass>();

	private List<GroupCompass> m_GroupCompasses = new List<GroupCompass>();

	private Dictionary<GameObject, SoloCompass> m_GoSoloCompassMap = new Dictionary<GameObject, SoloCompass>();

	private Dictionary<Sprite, GroupCompass> m_IconGroupCompassMap = new Dictionary<Sprite, GroupCompass>();

	private bool IsSoloCompassRegistered(SoloCompass soloCompass)
	{
		return m_SoloCompasses.Contains(soloCompass) && IsSoloCompassRegistered(soloCompass.constrainerGo);
	}

	private bool IsSoloCompassRegistered(GameObject go)
	{
		return m_GoSoloCompassMap.ContainsKey(go);
	}

	private bool IsGroupCompassRegistered(GroupCompass groupCompass)
	{
		return m_GroupCompasses.Contains(groupCompass) && IsGroupCompassRegistered(groupCompass.icon);
	}

	private bool IsGroupCompassRegistered(Sprite icon)
	{
		return m_IconGroupCompassMap.ContainsKey(icon);
	}

	private SoloCompass RegisterSoloCompass(SoloCompass soloCompass)
	{
		if (debug && soloCompass.name.Contains(debugName))
		{
			logger.Info("A RegisterSoloCompass");
		}
		if (IsSoloCompassRegistered(soloCompass))
		{
			return soloCompass;
		}
		if (debug && soloCompass.name.Contains(debugName))
		{
			logger.Info("B RegisterSoloCompass");
		}
		m_SoloCompasses.Add(soloCompass);
		m_GoSoloCompassMap.Add(soloCompass.constrainerGo, soloCompass);
		return soloCompass;
	}

	private GroupCompass RegisterGroupCompass(GroupCompass groupCompass)
	{
		if (IsGroupCompassRegistered(groupCompass))
		{
			return groupCompass;
		}
		m_GroupCompasses.Add(groupCompass);
		m_IconGroupCompassMap.Add(groupCompass.icon, groupCompass);
		return groupCompass;
	}

	private SoloCompass UnregisterSoloCompass(SoloCompass soloCompass)
	{
		if (debug && soloCompass.name.Contains(debugName))
		{
			logger.Info("A UnregisterSoloCompass");
		}
		if (!IsSoloCompassRegistered(soloCompass))
		{
			return soloCompass;
		}
		if (debug && soloCompass.name.Contains(debugName))
		{
			logger.Info("B UnregisterSoloCompass");
		}
		m_SoloCompasses.Remove(soloCompass);
		m_GoSoloCompassMap.Remove(soloCompass.constrainerGo);
		return soloCompass;
	}

	private GroupCompass UnregisterGroupCompass(GroupCompass groupCompass)
	{
		if (!IsGroupCompassRegistered(groupCompass))
		{
			return groupCompass;
		}
		m_GroupCompasses.Remove(groupCompass);
		m_IconGroupCompassMap.Remove(groupCompass.icon);
		return groupCompass;
	}

	public void DiscardSoloCompass(GameObject go)
	{
		DiscardSoloCompass(go, forceDestroy: false);
	}

	public void DiscardSoloCompass(GameObject go, bool forceDestroy)
	{
		if (debug && go.name.Contains(debugName))
		{
			logger.Info("A DiscardSoloCompass");
		}
		if (IsSoloCompassRegistered(go))
		{
			if (debug && go.name.Contains(debugName))
			{
				logger.Info("B DiscardSoloCompass");
			}
			GetSoloCompass(go).Discard(forceDestroy);
		}
	}

	public void DiscardGroupCompass(Sprite icon)
	{
		DiscardGroupCompass(icon, forceDestroy: false);
	}

	public void DiscardGroupCompass(Sprite icon, bool forceDestroy)
	{
		if (IsGroupCompassRegistered(icon))
		{
			GetGroupCompass(icon).Discard();
		}
	}

	public SoloCompass GetOrCreateSoloCompass(GameObject go, Sprite icon, out Sprite previous)
	{
		if (IsSoloCompassRegistered(go))
		{
			if (debug && go.name.Contains(debugName))
			{
				logger.Info("A GetOrCreateSoloCompass");
			}
			SoloCompass soloCompass = GetSoloCompass(go);
			previous = soloCompass.icon;
			soloCompass.icon = icon;
			return soloCompass;
		}
		if (debug && go.name.Contains(debugName))
		{
			logger.Info("B GetOrCreateSoloCompass");
		}
		previous = icon;
		return CreateSoloCompass(go, icon);
	}

	public GroupCompass GetOrCreateGroupCompass(Sprite icon, string name)
	{
		if (IsGroupCompassRegistered(icon))
		{
			if (debug && name.Contains(debugName))
			{
				logger.Info("A GetOrCreateGroupCompass");
			}
			return GetGroupCompass(icon);
		}
		if (debug && name.Contains(debugName))
		{
			logger.Info("B GetOrCreateGroupCompass");
		}
		return CreateGroupCompass(icon, name);
	}

	private SoloCompass GetSoloCompass(GameObject go)
	{
		return m_GoSoloCompassMap[go];
	}

	private GroupCompass GetGroupCompass(Sprite icon)
	{
		return m_IconGroupCompassMap[icon];
	}

	private SoloCompass CreateSoloCompass(GameObject go, Sprite icon)
	{
		if (debug && base.name.Contains(debugName))
		{
			logger.Info("A CreateSoloCompass");
		}
		if (m_SoloCompassCloneable == null)
		{
			return null;
		}
		if (debug && base.name.Contains(debugName))
		{
			logger.Info("B CreateSoloCompass");
		}
		GameObject gameObject = Object.Instantiate(m_SoloCompassCloneable, Vector3.zero, Quaternion.identity);
		gameObject.SetActive(value: true);
		gameObject.transform.SetParent(m_SoloCompassCloneBin.transform);
		gameObject.name = go.name + " SoloCompass *";
		SoloCompass component = gameObject.GetComponent<SoloCompass>();
		component.DoCreate(icon, go, delegate(SoloCompass i)
		{
			OnAfterCreateSoloCompass(i);
		}, delegate(SoloCompass i)
		{
			OnBeforeKillSoloCompass(i);
		});
		if (debug && base.name.Contains(debugName))
		{
			logger.Info("C CreateSoloCompass");
		}
		return component;
	}

	private GroupCompass CreateGroupCompass(Sprite icon, string name)
	{
		if (m_GroupCompassCloneable == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(m_GroupCompassCloneable, Vector3.zero, Quaternion.identity);
		gameObject.SetActive(value: true);
		gameObject.transform.SetParent(m_GroupCompassCloneBin.transform);
		gameObject.name = name + " GroupCompass *";
		GroupCompass component = gameObject.GetComponent<GroupCompass>();
		component.DoCreate(icon, delegate(GroupCompass i)
		{
			OnAfterCreateGroupCompass(i);
		}, delegate(GroupCompass i)
		{
			OnBeforeKillGroupCompass(i);
		});
		return component;
	}

	public void OnAfterCreateSoloCompass(SoloCompass soloCompass)
	{
		RegisterSoloCompass(soloCompass);
	}

	public void OnAfterCreateGroupCompass(GroupCompass groupCompass)
	{
		RegisterGroupCompass(groupCompass);
	}

	public void OnBeforeKillSoloCompass(SoloCompass soloCompass)
	{
		UnregisterSoloCompass(soloCompass);
	}

	public void OnBeforeKillGroupCompass(GroupCompass groupCompass)
	{
		UnregisterGroupCompass(groupCompass);
	}
}
