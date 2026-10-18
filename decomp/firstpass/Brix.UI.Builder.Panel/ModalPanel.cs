using System;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class ModalPanel : BaseSocketLayoutPanel<ModalPanelComponent, ModalPanel.Socket>
{
	public enum Variant
	{
		Default,
		Reskin
	}

	public enum Socket
	{
		Content,
		Header,
		Footer
	}

	private Factory.AssetKey _assetKey = Panels.Modal;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public ModalPanel(bool luaDriven = false)
		: base((Action<ModalPanelComponent>)null, (Action<ModalPanelComponent>)null, luaDriven)
	{
	}

	public ModalPanel(Action<ModalPanelComponent> onReset, Action<ModalPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public static ModalPanel LuaNew()
	{
		return new ModalPanel(luaDriven: true);
	}

	public static ModalPanel New(Action<ModalPanelComponent> onReset = null, Action<ModalPanelComponent> onRefresh = null)
	{
		return new ModalPanel(onReset, onRefresh);
	}

	public ModalPanel As(Variant variant)
	{
		switch (variant)
		{
		case Variant.Default:
			_assetKey = Panels.Modal;
			break;
		case Variant.Reskin:
			_assetKey = Panels.Modal;
			break;
		}
		return this;
	}

	public override RectTransform GetTrsAtSocket(Socket socket)
	{
		return socket switch
		{
			Socket.Content => base.Instance._contentBin, 
			Socket.Header => base.Instance._headerBin, 
			Socket.Footer => base.Instance._footerBin, 
			_ => throw new NotImplementedException(), 
		};
	}
}
