using System.Reflection;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableBlueprintBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableBlueprintBehaviour blueprintBehaviour;

	protected override SelectableBehaviour behaviour => blueprintBehaviour;

	[ContextMenu("FactorySetup")]
	private void FactorySetup()
	{
		FieldInfo field = typeof(SelectableBlueprintBehaviour).GetField("blueprint", BindingFlags.Instance | BindingFlags.NonPublic);
		field.SetValue(behaviour, GetComponent<Blueprint>());
		UISelectable component = GetComponent<UISelectable>();
		FieldInfo field2 = typeof(UISelectable).GetField("injector", BindingFlags.Instance | BindingFlags.NonPublic);
		field2.SetValue(component, this);
		FieldInfo field3 = typeof(UISelectable).GetField("useInjector", BindingFlags.Instance | BindingFlags.NonPublic);
		field3.SetValue(component, true);
	}
}
