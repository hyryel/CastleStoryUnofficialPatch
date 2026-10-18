using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class ThoughtBubble : Overhead, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	[SerializeField]
	private Text thought;

	[SerializeField]
	private Animator animator;

	private float until;

	public override int PreferedIndex => 1;

	private bool AnimatorReady => base.gameObject.activeInHierarchy && animator != null;

	public string Text
	{
		set
		{
			if (thought != null)
			{
				thought.text = value;
			}
		}
	}

	public void Init()
	{
	}

	public void SetThought(string text, bool important, bool forceRefresh = false)
	{
		if (forceRefresh || thought.text != text || !base.Show)
		{
			thought.text = text;
			float size = thought.preferredWidth * 1.2f;
			thought.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
			base.Show = true;
			if (AnimatorReady)
			{
				animator.SetTrigger("Show");
			}
		}
		if (important)
		{
			base.ForceShow = true;
			until = Time.time + 2f;
			if (!base.enabled)
			{
				base.enabled = true;
			}
		}
	}

	private void Update()
	{
		if (Time.time > until)
		{
			base.ForceShow = false;
			overhead.ShowOrHide();
			if (base.enabled)
			{
				base.enabled = false;
			}
		}
	}

	public void NoThought()
	{
		Text = string.Empty;
		base.Show = false;
		if (AnimatorReady)
		{
			animator.SetTrigger("Hide");
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		overhead = null;
		Text = string.Empty;
		base.Show = false;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		NoThought();
		overhead = null;
	}
}
