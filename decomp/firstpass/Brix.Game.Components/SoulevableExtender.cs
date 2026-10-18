using UnityEngine;

namespace Brix.Game.Components;

public static class SoulevableExtender
{
	public static void Soulever(this GameComponent c)
	{
		c.GetComponent<ISoulevable>()?.Soulever();
	}

	public static void Soulever(this GameObject c)
	{
		c.GetComponent<ISoulevable>()?.Soulever();
	}

	public static void Deposer(this GameComponent c)
	{
		c.GetComponent<ISoulevable>()?.Deposer();
	}

	public static void Deposer(this GameObject c)
	{
		c.GetComponent<ISoulevable>()?.Deposer();
	}
}
