using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Storage;

public class RecepteurDisplay : GameComponent
{
	[Serializable]
	public class DisplayElement
	{
		[Serializable]
		public class ConditionElement
		{
			[SerializeField]
			private string relatedAdjectiveSerialised = "Adjectif";

			private AdjectiveKey relatedAdjectiveCached;

			[SerializeField]
			public ElementCondition displayCondition;

			[SerializeField]
			public int conditionNumber;

			[SerializeField]
			public string customCondition;

			public AdjectiveKey relatedAdjective
			{
				get
				{
					if (string.IsNullOrEmpty(relatedAdjectiveSerialised))
					{
						relatedAdjective = AdjectiveKey.AllKeys[0];
					}
					else if (relatedAdjectiveCached == null)
					{
						relatedAdjectiveCached = AdjectiveKey.AllKeys.Slinq().FirstOrDefault((AdjectiveKey x) => x.codeName == relatedAdjectiveSerialised);
					}
					return relatedAdjectiveCached;
				}
				set
				{
					relatedAdjectiveCached = value;
					relatedAdjectiveSerialised = value.codeName;
				}
			}

			public bool TestCondition(Recepteur targetRecepteur)
			{
				Adjectif adjectif = relatedAdjective.GetAdjectif();
				if (displayCondition == ElementCondition.AlwaysOff)
				{
					return false;
				}
				if (displayCondition == ElementCondition.AlwaysOn)
				{
					return true;
				}
				if (displayCondition == ElementCondition.WhenHas)
				{
					return targetRecepteur.ContentDescription.Has(adjectif);
				}
				if (displayCondition == ElementCondition.WhenHasNone)
				{
					return !targetRecepteur.ContentDescription.Has(adjectif);
				}
				if (displayCondition == ElementCondition.WhenGreater)
				{
					return targetRecepteur.ContentDescription.Value(adjectif) > conditionNumber;
				}
				if (displayCondition == ElementCondition.WhenGreaterOrEqual)
				{
					return targetRecepteur.ContentDescription.Value(adjectif) >= conditionNumber;
				}
				if (displayCondition == ElementCondition.WhenEqual)
				{
					return targetRecepteur.ContentDescription.Value(adjectif) == conditionNumber;
				}
				if (displayCondition == ElementCondition.WhenLessOrEqual)
				{
					return targetRecepteur.ContentDescription.Value(adjectif) <= conditionNumber;
				}
				if (displayCondition == ElementCondition.WhenLess)
				{
					return targetRecepteur.ContentDescription.Value(adjectif) < conditionNumber;
				}
				return false;
			}

			public ConditionElement GetCopy()
			{
				ConditionElement conditionElement = new ConditionElement();
				conditionElement.relatedAdjective = relatedAdjective;
				conditionElement.conditionNumber = conditionNumber;
				conditionElement.displayCondition = displayCondition;
				return conditionElement;
			}
		}

		[SerializeField]
		public UnityEngine.Object targetObject;

		[SerializeField]
		public List<ConditionElement> conditions = new List<ConditionElement>
		{
			new ConditionElement()
		};

		[SerializeField]
		public ConditionLink conditionLink;

		public void UpdateDisplay(Recepteur targetRecepteur)
		{
			bool toggle = false;
			if (conditionLink == ConditionLink.And)
			{
				toggle = true;
			}
			else if (conditionLink == ConditionLink.Or)
			{
				toggle = false;
			}
			for (int i = 0; i != conditions.Count; i++)
			{
				if (conditionLink == ConditionLink.And)
				{
					if (!conditions[i].TestCondition(targetRecepteur))
					{
						toggle = false;
						break;
					}
				}
				else if (conditionLink == ConditionLink.Or && conditions[i].TestCondition(targetRecepteur))
				{
					toggle = true;
					break;
				}
			}
			ToggleDisplay(toggle);
		}

		public void ToggleDisplay(bool toggle)
		{
			if (targetObject is GameObject)
			{
				(targetObject as GameObject).SetActive(toggle);
			}
			else if (targetObject is StateRenderer)
			{
				StateRenderer stateRenderer = (StateRenderer)targetObject;
				stateRenderer.ProgressOverride(toggle);
			}
			else
			{
				(targetObject as Behaviour).enabled = toggle;
			}
		}

		public DisplayElement GetCopy()
		{
			DisplayElement displayElement = new DisplayElement();
			displayElement.targetObject = targetObject;
			displayElement.conditions = new List<ConditionElement>();
			foreach (ConditionElement condition in conditions)
			{
				displayElement.conditions.Add(condition.GetCopy());
			}
			return displayElement;
		}
	}

	public enum ElementCondition
	{
		AlwaysOff,
		AlwaysOn,
		WhenHas,
		WhenHasNone,
		WhenGreater,
		WhenGreaterOrEqual,
		WhenEqual,
		WhenLessOrEqual,
		WhenLess,
		Custom
	}

	public enum ConditionLink
	{
		And,
		Or
	}

	public Recepteur targetRecepteur;

	public DisplayElement defaultDummy = new DisplayElement();

	public List<DisplayElement> displayElements = new List<DisplayElement>();

	protected override void Start()
	{
		base.Start();
		targetRecepteur.Changed.Connect(delegate
		{
			base.enabled = true;
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void Update()
	{
		base.enabled = false;
		UpdateElements();
	}

	private void UpdateElements()
	{
		for (int i = 0; i != displayElements.Count; i++)
		{
			displayElements[i].UpdateDisplay(targetRecepteur);
		}
	}
}
