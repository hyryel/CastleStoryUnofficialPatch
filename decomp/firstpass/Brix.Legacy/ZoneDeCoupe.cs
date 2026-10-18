using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Lifecycle.Pooling;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Legacy;

[JsonObject(MemberSerialization.OptIn)]
public class ZoneDeCoupe : MonoBehaviour, PieMenuable
{
	[SerializeField]
	private Material TreeMaterial;

	public IntClasse arbres;

	public Project project;

	private SignalConnection TreeChangedConnection = SignalConnection.Empty;

	private int highlight_id;

	private int ancienRayon = -1;

	private int ancienIndex = -1;

	private int ancienArbreCount;

	private bool etaitActif;

	public Beacon Balise => project.Beacon;

	public XYZ position => Balise.position;

	public int Radius => (int)Balise.radius;

	private void Awake()
	{
		project = GetComponent<Project>();
	}

	private void Start()
	{
		CalculerArbres();
		highlight_id = CircleHighlighter.GetId();
		TreeChangedConnection = GameSignals.Connect(GameSignals.TreeChanged, delegate(XYZ p)
		{
			OnTreeChanged(p);
		});
	}

	private void OnDestroy()
	{
		CircleHighlighter.Clear(highlight_id);
		TreeChangedConnection.Disconnect();
	}

	public void OnTreeChanged(XYZ p)
	{
		if (TreeRegistry.IsInRadius(p, position, Radius))
		{
			CalculerArbres();
		}
	}

	public void CalculerArbres()
	{
		if (arbres == null)
		{
			arbres = new IntClasse();
		}
		else
		{
			arbres.Clear();
		}
		foreach (XYZ item in TreeRegistry.Instance.GetTreesInRadius(position, Radius).NoLeak())
		{
			arbres.Add(item.ToScalaire());
		}
	}

	private void Update()
	{
		if (arbres == null)
		{
			return;
		}
		if ((bool)UIGameObserver.projects.CurrentSelected && UIGameObserver.projects.CurrentSelected == project)
		{
			if (!etaitActif || Radius != ancienRayon || position != XYZ.FromScalaire(ancienIndex) || arbres.Count != ancienArbreCount)
			{
				ancienRayon = Radius;
				ancienIndex = position.ToScalaire();
				ancienArbreCount = arbres.Count;
				etaitActif = true;
				CalculerArbres();
				CircleHighlighter.Clear(highlight_id);
				for (int i = 0; i < arbres.Count; i++)
				{
					CircleHighlight circleHighlight = CircleHighlighter.HighlightPosition(highlight_id, Brix.Engine.Abstract.Voxel.GetVecteur(arbres[i]), TreeMaterial, 1.5f);
					circleHighlight.transform.parent = base.transform;
				}
			}
		}
		else if (etaitActif)
		{
			etaitActif = false;
			CircleHighlighter.Clear(highlight_id);
		}
	}

	public void Delete()
	{
		ObjectPoolSingleton.Release(base.gameObject);
	}

	public bool CheckTag(string tag)
	{
		return "ZoneDeCoupe".Contains(tag);
	}
}
