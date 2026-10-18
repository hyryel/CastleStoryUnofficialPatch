using System.Reflection;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableProjectBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableProjectBehaviour projectBehaviour;

	protected override SelectableBehaviour behaviour => projectBehaviour;

	[ContextMenu("FactorySetup")]
	private void FactorySetup()
	{
		FieldInfo field = typeof(SelectableProjectBehaviour).GetField("project", BindingFlags.Instance | BindingFlags.NonPublic);
		field.SetValue(behaviour, GetComponent<Project>());
		UISelectable component = GetComponent<UISelectable>();
		FieldInfo field2 = typeof(UISelectable).GetField("injector", BindingFlags.Instance | BindingFlags.NonPublic);
		field2.SetValue(component, this);
		FieldInfo field3 = typeof(UISelectable).GetField("useInjector", BindingFlags.Instance | BindingFlags.NonPublic);
		field3.SetValue(component, true);
	}
}
