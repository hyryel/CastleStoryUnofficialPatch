using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components.Tunnel;
using Smooth.Slinq;
using UnityEngine;

public class TunnelUI : MonoBehaviour
{
	[SerializeField]
	private GameObject Units;

	[SerializeField]
	private GameObject Volumes;

	private HashSet<TunnelUnitUI> unitUIs = new HashSet<TunnelUnitUI>();

	private void Start()
	{
		if (UIGameObserver.projects.CurrentSelected == GetComponent<Project>())
		{
			Units = GetComponentsInChildren<TunnelVolume>(includeInactive: true).Slinq().First().gameObject;
		}
	}

	public void AddUnit(TunnelUnitUI unit)
	{
		if (unit != null)
		{
			unitUIs.Add(unit);
		}
	}

	public void RemoveUnit(TunnelUnitUI unit)
	{
		unitUIs.Remove(unit);
	}

	public void Show(bool value)
	{
		Volumes.SetActive(value);
		Units.SetActive(value);
	}
}
