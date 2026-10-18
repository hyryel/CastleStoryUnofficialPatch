using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutElementToggle : RadialLayoutElement
{
	public enum Group
	{
		_None,
		Alpha,
		Beta,
		Gamma
	}

	private static BrixLogger logger = new BrixLogger(typeof(RadialLayoutElementToggle).FullName);

	public Toggle toggle;

	public ToggleGroup toggleGroup;

	public Group group;

	public override void Init(RadialLayoutGroup radialLayoutGroup)
	{
		base.Init(radialLayoutGroup);
		toggle = ((!(toggle != null)) ? base.gameObject.GetComponent<Toggle>() : toggle);
		toggle = ((!(toggle != null)) ? base.gameObject.AddComponent<Toggle>() : toggle);
		if (toggleGroup == null)
		{
			toggleGroup = radialLayoutGroup.GetComponent<ToggleGroup>();
		}
		if (toggleGroup != null)
		{
			toggle.group = toggleGroup;
		}
		InitSignals();
	}

	public override void InitSignals()
	{
		base.InitSignals();
	}

	public override void InitBgRawImage()
	{
		base.InitBgRawImage();
		bgRawImage.material = new Material(radialLayoutGroup.segmentMaterial);
	}

	public override void OnMouseDown(float angle)
	{
		base.OnMouseDown(angle);
		if (RadialLayoutElement.isDebug)
		{
			logger.Info(base.name + ".OnMouseDown(" + angle + ")\n");
		}
		if (base.actionInputEvent == RadialLayoutGroup.InputEvent.Press)
		{
			DoToggle();
		}
	}

	public override void OnMouseUp(float angle)
	{
		base.OnMouseUp(angle);
		if (RadialLayoutElement.isDebug)
		{
			logger.Info(base.name + ".OnMouseUp(" + angle + ")\n");
		}
		if (base.actionInputEvent == RadialLayoutGroup.InputEvent.Release)
		{
			DoToggle();
		}
	}

	public void OnToggle(bool value)
	{
		if (RadialLayoutElement.isDebug)
		{
			logger.Info(base.name + ".OnToggle(" + value + ")\n");
		}
		base.isToggle = value;
		UpdateBackground();
	}

	private void DoToggle()
	{
		bool flag = base.isToggle;
		bool flag2 = !flag;
		if (toggleGroup == null || toggleGroup.allowSwitchOff || flag2)
		{
			radialLayoutGroup.OnToggle(this, flag2);
		}
	}
}
