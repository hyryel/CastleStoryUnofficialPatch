using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Input;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

public class Containee : MonoBehaviour
{
	public enum State
	{
		None,
		Opened,
		Closed
	}

	private static BrixLogger logger = new BrixLogger(typeof(Containee).FullName);

	public State currentState = State.Closed;

	public State targetState;

	public Container container;

	public static bool isDebug = false;

	public RawImage image;

	public Image spriteImage;

	public Image spriteImage2;

	public Text title;

	public Text subtitle;

	public Text subsubtitle;

	public Text subsubsubtitle;

	public Text inputField;

	public InputField actualInputField;

	public Toggle toggle;

	public Text toggleText;

	public Button button;

	public Button altButton;

	public Slider slider;

	public Text sliderValueTitle;

	public Image toggleImage;

	public Image checkboxImage;

	public Image checkmarkImage;

	public LayoutElement layoutElement;

	public MenuSound.Suffix soundHoverTrue;

	public MenuSound.Suffix soundHoverFalse;

	public MenuSound.Suffix soundHoverDisabledTrue;

	public MenuSound.Suffix soundHoverDisabledFalse;

	public MenuSound.Suffix soundAction;

	public MenuSound.Suffix soundActionNegative;

	public MenuSound.Suffix soundAltAction;

	public MenuSound.Suffix soundAltActionNegative;

	public MenuSound.Suffix soundToggleTrue;

	public MenuSound.Suffix soundToggleFalse;

	public MenuSound.Suffix soundScrollPositive;

	public MenuSound.Suffix soundScrollNegative;

	public MenuSound.Suffix soundText;

	public MenuSound.Suffix soundOpen;

	public MenuSound.Suffix soundClose;

	[SerializeField]
	private InterpolatorTargetable m_InterpolatorTargetable;

	public List<Action> actionsOnSelect = new List<Action>();

	public virtual bool playSounds => true;

	public InterpolatorTargetable interpolatorTargetable
	{
		get
		{
			return m_InterpolatorTargetable;
		}
		set
		{
			m_InterpolatorTargetable = value;
		}
	}

	protected virtual void OnEnable()
	{
		interpolatorTargetable = interpolatorTargetable ?? base.gameObject.GetComponent<InterpolatorTargetable>();
	}

	public void Select()
	{
		if (container != null)
		{
			container.Select(this);
		}
	}

	public void Hover(bool flag)
	{
		if (container != null)
		{
			container.Hover(this, flag);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel((!flag) ? MenuSound.Suffix.RolloverOff : MenuSound.Suffix.RolloverOn, container));
		}
	}

	public void Click()
	{
		if (container != null)
		{
			container.Click(this);
			container.Select(this);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Suffix.Select, container));
		}
	}

	public void AltClick()
	{
		if (container != null)
		{
			container.AltClick(this);
			container.Select(this);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Suffix.Select, container));
		}
	}

	public void Toggle(bool flag)
	{
		if (container != null)
		{
			container.Toggle(this, flag);
			container.Select(this);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel((!flag) ? MenuSound.Suffix.Toggle : MenuSound.Suffix.Toggle, container));
		}
	}

	public void Scroll(float value)
	{
		if (container != null)
		{
			container.Scroll(this, value);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel((!(value > 0f)) ? MenuSound.Suffix._None : MenuSound.Suffix._None, container));
		}
	}

	public void ScrollEnd()
	{
		if (container != null)
		{
			container.ScrollEnd(this, slider.value);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel((!(slider.value > 0f)) ? MenuSound.Suffix._None : MenuSound.Suffix._None, container));
		}
	}

	public void Text(string text)
	{
		if (container != null)
		{
			container.Text(this, text);
		}
		if (playSounds)
		{
			SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Suffix.ChatSend, container));
		}
	}

	public void Key(KeyCode keyCode)
	{
		if (!(container == null) && keyCode == KeyCode.F10 && (InputModeController.GetCurrentMode() == InputMode.command || InputModeController.GetCurrentMode() == InputMode.menu))
		{
			ToggleActivation();
		}
	}

	[ContextMenu("EnableActivation")]
	public void EnableActivation()
	{
		RelayActivationAction(Container.Item.ActivationAction.Activate);
	}

	[ContextMenu("DisableActivation")]
	public void DisableActivation()
	{
		RelayActivationAction(Container.Item.ActivationAction.Deactivate);
	}

	[ContextMenu("ToggleActivation")]
	public void ToggleActivation()
	{
		RelayActivationAction(Container.Item.ActivationAction.Toggle);
	}

	public void ToggleActivation(bool flag)
	{
		if (flag)
		{
			EnableActivation();
		}
		else
		{
			DisableActivation();
		}
	}

	public void RelayActivationAction(Container.Item.ActivationAction activationAction)
	{
		if (container == null)
		{
			logger.ErrorFormat("Container missing from containee: {0}", base.name);
		}
		else
		{
			container.ExecActivationAction(this, activationAction);
		}
	}
}
