using System;
using System.Reflection;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game;

public class ScreenSignaler : UpdatableGameComponent<ScreenSignaler>
{
	private delegate void GetPlanes(Plane[] planes, Matrix4x4 matrix);

	public Signal Enter = new Signal();

	public Signal Exit = new Signal();

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool Visible;

	private Camera _cam;

	private Renderer[] _renderers;

	private bool _isVisible;

	private static GetPlanes planeGetter;

	private static Plane[] planes = new Plane[6];

	protected override bool IsUpdatedWithUnscaledTime => true;

	private bool IsVisible => base.gameObject.activeSelf && base.gameObject.activeInHierarchy && (HasZeroRenderers || IsAnyRenderedByCamera(_cam, _renderers));

	private bool HasZeroRenderers => _renderers.Length == 0;

	protected override void Start()
	{
		base.Start();
		InitPlaneGetter();
		_cam = Camera.main;
		_renderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		OnVisibleChange();
	}

	protected override void BrixSlowUpdate()
	{
		TryOnVisibleChange();
	}

	private void TryOnVisibleChange()
	{
		if (_isVisible != IsVisible)
		{
			OnVisibleChange();
		}
	}

	private void OnVisibleChange()
	{
		if (!(base.gameObject == null))
		{
			_isVisible = IsVisible;
			if (_isVisible)
			{
				Enter.Invoke();
			}
			else
			{
				Exit.Invoke();
			}
		}
	}

	public bool GetIsVisible()
	{
		return _isVisible;
	}

	private static bool IsRendererActive(Renderer renderer)
	{
		if (renderer == null)
		{
			return false;
		}
		return renderer.enabled && renderer.gameObject.activeSelf && renderer.gameObject.activeInHierarchy;
	}

	protected override void BrixUpdate()
	{
		Visible = IsAnyRenderedByCamera(_cam, _renderers);
	}

	private static void GetPlane(ref Plane[] planes, Camera cam)
	{
		planeGetter(planes, cam.projectionMatrix * cam.worldToCameraMatrix);
	}

	private static void InitPlaneGetter()
	{
		if (planeGetter == null)
		{
			MethodInfo method = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic);
			Delegate obj = Delegate.CreateDelegate(typeof(GetPlanes), method);
			planeGetter = (GetPlanes)obj;
		}
	}

	private static bool IsAnyRenderedByCamera(Camera cam, Renderer[] renderers)
	{
		GetPlane(ref planes, cam);
		return renderers.Slinq().Any((Renderer r, Plane[] p) => IsRendererActive(r) && GeometryUtility.TestPlanesAABB(p, r.bounds), planes);
	}
}
