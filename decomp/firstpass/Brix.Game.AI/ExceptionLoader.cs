using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Brix.Game.AI;

public class ExceptionLoader : MonoBehaviour
{
	[ContextMenu("Load")]
	private void Awake()
	{
		Type[] nestedTypes = typeof(Exceptions).GetNestedTypes();
		foreach (Type type in nestedTypes)
		{
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		}
	}
}
