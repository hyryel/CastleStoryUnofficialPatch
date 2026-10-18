using Brix.Game.Components;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

public class GripSocket : MonoBehaviour
{
	[Matrix4x4PropertyDrawer(true, true, true)]
	public Matrix4x4 socketMatrix = Matrix4x4.identity;

	private LocalFactionVision _vision;

	private Vector3 other = new Vector3(0.1f, 0.1f, 0.1f);

	private bool collapsed;

	private LocalFactionVision vision
	{
		get
		{
			if (_vision == null)
			{
				_vision = this.GetComponentInParent<LocalFactionVision>(includeInactive: true);
			}
			return _vision;
		}
	}

	public void Collapse(bool collapse)
	{
		if (collapse != collapsed)
		{
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = other;
			other = localScale;
		}
	}

	public void AdjustFactionVision()
	{
		bool parameter = vision.IsVisibleForMe();
		base.transform.GetComponentsInChildren<MeshRenderer>().Slinq().ForEach(delegate(MeshRenderer obj, bool _visible)
		{
			obj.enabled = _visible;
		}, parameter);
	}
}
