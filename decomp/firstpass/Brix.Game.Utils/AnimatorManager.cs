using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Utils;

public class AnimatorManager : BrixSingleton<AnimatorManager>
{
	private readonly Dictionary<Animator, AnimatorAction> _job = new Dictionary<Animator, AnimatorAction>();

	private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

	private static Dictionary<Animator, AnimatorAction> Job => BrixSingleton<AnimatorManager>.Instance._job;

	public static void Add(Animator animator, AnimatorAction animatorAction)
	{
		Job[animator] = animatorAction;
	}

	public static void Remove(Animator animator)
	{
		if (!(BrixSingleton<AnimatorManager>.Instance == null) && !(animator == null))
		{
			Job.Remove(animator);
		}
	}

	private static void RemoveAll()
	{
		Job.Clear();
	}

	private static bool Apply(Animator animator, AnimatorAction animatorAction)
	{
		if (!animator.isActiveAndEnabled || !animator.isInitialized)
		{
			return false;
		}
		animatorAction.Apply(animator);
		return true;
	}

	private static void ApplyAll()
	{
		List<Animator> list = CollectionPool.RequestList<Animator>();
		foreach (KeyValuePair<Animator, AnimatorAction> item in Job)
		{
			if (Apply(item.Key, item.Value))
			{
				list.Add(item.Key);
			}
		}
		foreach (Animator item2 in list)
		{
			Job.Remove(item2);
		}
		list.Dispose();
	}

	public IEnumerator OnEndOfFrame()
	{
		while (true)
		{
			yield return _waitForEndOfFrame;
			if (Job.Count != 0)
			{
				ApplyAll();
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(OnEndOfFrame());
	}
}
