using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class Exclusion
{
	public const float exclusionLifeTime = 30f;

	public Tuple<GameObject, GameObject> Members;

	public float ExpirationTime;

	public Exclusion(GameObject A, GameObject B)
	{
		Members = BuildTuple(A, B);
		ExpirationTime = Time.time + 30f;
	}

	public static Tuple<GameObject, GameObject> BuildTuple(GameObject A, GameObject B)
	{
		if (A.GetInstanceID() > B.GetInstanceID())
		{
			return new Tuple<GameObject, GameObject>(A, B);
		}
		return new Tuple<GameObject, GameObject>(B, A);
	}
}
