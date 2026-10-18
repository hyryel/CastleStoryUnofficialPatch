using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class TextureWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected RawImage _rawImage;

	private Outline _outline1;

	private Outline _outline2;

	private Outline _outline3;

	private Outline _outline4;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetEnabled>())
		{
			_rawImage.enabled = (w as IGetEnabled).GetEnabled();
		}
		if (w.Is<IGetTexture>())
		{
			_rawImage.texture = (w as IGetTexture).GetTexture();
		}
		if (w.Is<IGetColor>())
		{
			_rawImage.color = (w as IGetColor).GetColor();
		}
		if (w.Is<IGetRaycasts>())
		{
			_rawImage.raycastTarget = (w as IGetRaycasts).GetRaycasts();
		}
		if (w.Is<IGetUvRect>())
		{
			Vector4 uvRect = (w as IGetUvRect).GetUvRect();
			_rawImage.uvRect = new Rect(uvRect.x, uvRect.y, uvRect.z, uvRect.w);
		}
		if (w.Is<IGetOutlines>())
		{
			GetOutlines((w as IGetOutlines).GetOutlines());
		}
		else
		{
			GetOutlines(DynValue.Nil);
		}
		return true;
	}

	private void GetOutlines(DynValue d)
	{
		if (d.IsNil())
		{
			if (_outline1 != null)
			{
				_outline1.enabled = false;
			}
			if (_outline2 != null)
			{
				_outline2.enabled = false;
			}
			if (_outline3 != null)
			{
				_outline3.enabled = false;
			}
			if (_outline4 != null)
			{
				_outline4.enabled = false;
			}
			return;
		}
		if (_outline1 == null)
		{
			_outline1 = base.gameObject.AddComponent<Outline>();
		}
		if (_outline2 == null)
		{
			_outline2 = base.gameObject.AddComponent<Outline>();
		}
		if (_outline3 == null)
		{
			_outline3 = base.gameObject.AddComponent<Outline>();
		}
		if (_outline4 == null)
		{
			_outline4 = base.gameObject.AddComponent<Outline>();
		}
		Table table = d.Table;
		DynValue dynValue = table.Get(1);
		DynValue dynValue2 = table.Get(2);
		DynValue dynValue3 = table.Get(3);
		DynValue dynValue4 = table.Get(4);
		bool flag = dynValue.IsNotNil() && dynValue.Table != null;
		bool flag2 = dynValue2.IsNotNil() && dynValue2.Table != null;
		bool flag3 = dynValue3.IsNotNil() && dynValue3.Table != null;
		bool flag4 = dynValue4.IsNotNil() && dynValue4.Table != null;
		_outline1.enabled = flag;
		_outline2.enabled = flag2;
		_outline3.enabled = flag3;
		_outline4.enabled = flag4;
		if (flag)
		{
			Table table2 = dynValue.Table;
			int num = Mathf.RoundToInt((float)table2.Get("size").Number);
			_outline1.effectDistance = new Vector2(num, -num);
			_outline1.effectColor = table2.Get("color").ToObject<Color>();
		}
		if (flag2)
		{
			Table table3 = dynValue2.Table;
			int num2 = Mathf.RoundToInt((float)table3.Get("size").Number);
			_outline2.effectDistance = new Vector2(num2, -num2);
			_outline2.effectColor = table3.Get("color").ToObject<Color>();
		}
		if (flag3)
		{
			Table table4 = dynValue3.Table;
			int num3 = Mathf.RoundToInt((float)table4.Get("size").Number);
			_outline3.effectDistance = new Vector2(num3, -num3);
			_outline3.effectColor = table4.Get("color").ToObject<Color>();
		}
		if (flag4)
		{
			Table table5 = dynValue4.Table;
			int num4 = Mathf.RoundToInt((float)table5.Get("size").Number);
			_outline4.effectDistance = new Vector2(num4, -num4);
			_outline4.effectColor = table5.Get("color").ToObject<Color>();
		}
	}
}
