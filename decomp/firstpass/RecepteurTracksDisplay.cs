using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Smooth.Slinq;
using UnityEngine;

public class RecepteurTracksDisplay : GameComponent
{
	[Serializable]
	public class DisplayTrack
	{
		[SerializeField]
		public string adjectif;

		[SerializeField]
		public List<GameObject> gos;

		private List<Adjectif> adjs;

		public bool Valid => adjs != null;

		public void Init()
		{
			if (!string.IsNullOrEmpty(adjectif))
			{
				adjs = adjectif.Split('+').Select(Adjectif.StringToAdjectif).NotNull()
					.ToList();
			}
		}

		public void UpdateDisplay(Recepteur targetRecepteur)
		{
			int num = adjs.Slinq().Aggregate(0, (int s, Adjectif a, Recepteur r) => s + r.ContentDescription.Value(a), targetRecepteur);
			for (int num2 = 0; num2 < gos.Count; num2++)
			{
				if ((bool)gos[num2])
				{
					gos[num2].SetActive(num2 < num);
				}
			}
		}
	}

	public Recepteur targetRecepteur;

	public List<DisplayTrack> DisplayTracks = new List<DisplayTrack>();

	[ContextMenu("Clean List")]
	private void CleanList()
	{
		DisplayTracks = DisplayTracks.Where((DisplayTrack t) => t.Valid).ToList();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		DisplayTracks.ForEach(delegate(DisplayTrack d)
		{
			d.Init();
		});
	}

	private void Update()
	{
		(from d in DisplayTracks.Slinq()
			where d.Valid
			select d).ForEach(delegate(DisplayTrack d, Recepteur r)
		{
			d.UpdateDisplay(r);
		}, targetRecepteur);
	}
}
