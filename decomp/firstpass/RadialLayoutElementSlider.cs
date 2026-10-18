using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutElementSlider : RadialLayoutElement
{
	public Slider slider;

	[Disabled]
	public float setValue;

	[Disabled]
	public bool queueSetValue;

	public override void Init(RadialLayoutGroup radialLayoutGroup)
	{
		base.Init(radialLayoutGroup);
		slider = ((!(slider != null)) ? base.gameObject.GetComponent<Slider>() : slider);
		slider = ((!(slider != null)) ? base.gameObject.AddComponent<Slider>() : slider);
		if (slider.fillRect == null)
		{
			GameObject gameObject = new GameObject("[RadialLayoutElementSlider] Slider Place Holder", typeof(RectTransform));
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.SetSiblingIndex(0);
			slider.fillRect = gameObject.transform as RectTransform;
		}
		slider.handleRect = ((!(slider.handleRect != null)) ? slider.fillRect : slider.handleRect);
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

	public override void UpdateBackground()
	{
		base.UpdateBackground();
		base.progress = slider.value;
	}

	public override void OnMouseDrag(float angle)
	{
		base.OnMouseDrag(angle);
		if (base.actionInputEvent == RadialLayoutGroup.InputEvent.Release)
		{
			if (isDragIn)
			{
				LateSetValue(angle);
			}
		}
		else if (isDrag && isTrueHover)
		{
			LateSetValue(angle);
		}
	}

	public override void OnMouseDown(float angle)
	{
		base.OnMouseDown(angle);
		if (base.actionInputEvent == RadialLayoutGroup.InputEvent.Press)
		{
			LateSetValue(angle);
		}
	}

	public override void OnMouseUp(float angle)
	{
		base.OnMouseUp(angle);
		if (base.actionInputEvent == RadialLayoutGroup.InputEvent.Release)
		{
			LateSetValue(angle);
		}
	}

	public void LateSetValue(float angle)
	{
		if (isHover || isDrag)
		{
			setValue = angle;
			setValue += base.angleOffset;
			setValue = (10000f + setValue) % 1f;
			float num = (1f - base.angleSpread) / 2f;
			setValue -= num;
			setValue /= base.angleSpread;
			float num2 = base.angleOffset;
			float num3 = base.baseAngleOffset + base.angleOffset + base.angleSpread;
			num3 = (10000f + num3) % 1f;
			num2 = 0f;
			num3 = 1f;
			setValue = ((!(setValue > num3)) ? setValue : num3);
			setValue = ((!(setValue < num2)) ? setValue : num2);
			queueSetValue = true;
		}
	}

	private void DoSetValue()
	{
		slider.value = setValue;
		UpdateBackground();
	}

	public override void Update()
	{
		base.Update();
		if (queueSetValue)
		{
			queueSetValue = false;
			DoSetValue();
		}
	}
}
