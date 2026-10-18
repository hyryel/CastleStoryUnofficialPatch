using System;
using System.Collections.Generic;
using UnityEngine;

public class ContaineeExtended : Containee, IPlug
{
	[SerializeField]
	private List<Plug> m_Plugs = new List<Plug>();

	private Dictionary<Type, Dictionary<string, Plug>> m_PlugMap = new Dictionary<Type, Dictionary<string, Plug>>();

	public List<Plug> Plugs => m_Plugs;

	public Dictionary<Type, Dictionary<string, Plug>> PlugMap => m_PlugMap;

	protected override void OnEnable()
	{
		base.OnEnable();
		this.ResetPlugCache();
	}
}
