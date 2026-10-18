using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class OkDialogContainerPopulator : MonoBehaviour, IPlug
{
	public class GenericDialog
	{
		public class Btn
		{
			public string _label;

			public string _otherLabel;

			public Action _action;

			public bool _isCheckbox;

			public Btn(string label, Action action, string otherLabel = "", bool isCheckBox = false)
			{
				_label = label;
				_otherLabel = otherLabel;
				_action = action;
				_isCheckbox = isCheckBox;
			}
		}

		public string _title;

		public string _content;

		public Btn[] _btns;

		public GenericDialog(string title, string content, Btn[] btns)
		{
			_title = title;
			_content = content;
			_btns = btns;
		}
	}

	public class TextureGenericDialog : GenericDialog
	{
		public Texture _texture;

		public TextureGenericDialog(string title, string content, Btn[] btns, Texture texture)
			: base(title, content, btns)
		{
			_texture = texture;
		}
	}

	public class SpriteGenericDialog : GenericDialog
	{
		public Sprite _sprite;

		public SpriteGenericDialog(string title, string content, Btn[] btns, Sprite sprite)
			: base(title, content, btns)
		{
			_sprite = sprite;
		}
	}

	public class LoginGenericDialog : TextureGenericDialog
	{
		public bool _defaultFlag;

		public string _defaultChanName;

		public string _defaultUserName;

		public string _defaultPassName;

		public Action<bool> _flagValidation;

		public Action<string> _chanNameValidation;

		public Action<string> _userNameValidation;

		public Action<string> _passNameValidation;

		public LoginGenericDialog(string title, string content, Btn[] btns, Texture texture, bool defaultFlag, string defaultChanName, string defaultUserName, string defaultPassName, Action<bool> flagValidation, Action<string> chanNameValidation, Action<string> userNameValidation, Action<string> passNameValidation)
			: base(title, content, btns, texture)
		{
			_defaultFlag = defaultFlag;
			_defaultChanName = defaultChanName;
			_defaultUserName = defaultUserName;
			_defaultPassName = defaultPassName;
			_flagValidation = flagValidation;
			_chanNameValidation = chanNameValidation;
			_userNameValidation = userNameValidation;
			_passNameValidation = passNameValidation;
		}
	}

	public class InputGenericDialog : TextureGenericDialog
	{
		public string _defaultInput;

		public Action<string> _inputValidation;

		public InputGenericDialog(string title, string content, Btn[] btns, Texture texture, string defaultInput, Action<string> inputValidation)
			: base(title, content, btns, texture)
		{
			_defaultInput = defaultInput;
			_inputValidation = inputValidation;
		}
	}

	public class RadioGenericDialog : GenericDialog
	{
		public string[] _radioBtnLabels;

		public int _selectedIndex;

		public Action<int> _inputValidation;

		public RadioGenericDialog(string title, string content, Btn[] btns, string[] radioBtnLabels, int selectedIndex, Action<int> inputValidation)
			: base(title, content, btns)
		{
			_radioBtnLabels = radioBtnLabels;
			_selectedIndex = selectedIndex;
			_inputValidation = inputValidation;
		}
	}

	public class StartDialog : GenericDialog
	{
		public string CheckboxText;

		public Action CheckboxAction;

		public Func<bool> CheckboxValueFunc;

		public StartDialog(string titleText, string contentText, string checkboxText, string okText, Action checkboxAction, Action okAction, Func<bool> checkboxValueFunc)
			: base(titleText, contentText, new Btn[1]
			{
				new Btn(okText, okAction, string.Empty)
			})
		{
			CheckboxText = checkboxText;
			CheckboxAction = checkboxAction;
			CheckboxValueFunc = checkboxValueFunc;
		}
	}

	[SerializeField]
	private List<Plug> m_Plugs = new List<Plug>();

	private Dictionary<Type, Dictionary<string, Plug>> m_PlugMap = new Dictionary<Type, Dictionary<string, Plug>>();

	private bool connected;

	public static OkDialogContainerPopulator ConnectedDialog;

	private SignalConnection _displayDialogConnection = SignalConnection.Empty;

	private SignalConnection _displayStartDialogConnection = SignalConnection.Empty;

	private SignalConnection _displayOkDialogConnection = SignalConnection.Empty;

	private SignalConnection _displayGenericDialogConnection = SignalConnection.Empty;

	private SignalConnection _displayDialogRadioSelectConnection = SignalConnection.Empty;

	private SignalConnection _displayDialogFlag = SignalConnection.Empty;

	private SignalConnection _displayDialogUserName = SignalConnection.Empty;

	private SignalConnection _displayDialogPassName = SignalConnection.Empty;

	private SignalConnection _closeDialog = SignalConnection.Empty;

	public GameObject keyBindGo;

	public Text keyBindTitleText;

	public Text keyBindSubtitleText;

	public Text keyBindPrimaryText;

	public Text keyBindAlternateText;

	public Text uiTitleText;

	public Text uiContentText;

	public RawImage uiContentRawImage;

	public Image uiContentImage;

	public InputField uiContentInputField;

	public GameObject uiStartCheckboxGo;

	public GameObject uiStartCheckboxImageGo;

	public Text uiStartCheckboxText;

	public Button uiStartCheckboxButton;

	public EventTrigger uiStartCheckboxEventTrigger;

	public InputField uiChanNameInputField;

	public Button uiChanNameEraseButton;

	public InputField uiUserNameInputField;

	public Button uiUserNameEraseButton;

	public InputField uiPassNameInputField;

	public Button uiPassNameEraseButton;

	public Containee mainPanel;

	public Container radioContainer;

	public Containee radioContainee;

	public List<ContaineeExtended> btnClones;

	public List<ContaineeExtended> radioBtnClones;

	public ContentSizeFitter uiRadioContentSizeFitter;

	public LayoutElementConstraint uiRadioLayoutElementConstraint;

	public AspectRatioFitter uiSpriteAspectRatioFitter;

	public LayoutElementConstraint uiSpriteLayoutElementConstraint;

	public AspectRatioFitter uiTextureAspectRatioFitter;

	public LayoutElementConstraint uiTextureLayoutElementConstraint;

	public ContentSizeFitter uiTextContentSizeFitter;

	public LayoutElementConstraint uiTextLayoutElementConstraint;

	public List<Plug> Plugs => m_Plugs;

	public Dictionary<Type, Dictionary<string, Plug>> PlugMap => m_PlugMap;

	private void Start()
	{
		TryConnect();
	}

	private void Awake()
	{
		TryConnect();
	}

	private void OnEnable()
	{
		TryConnect();
	}

	private void OnDisable()
	{
		TryDisconnect();
	}

	private void TryConnect()
	{
		TryDisconnect();
		if (!connected)
		{
			Connect();
		}
	}

	private void Connect()
	{
		_displayDialogConnection = GameSignals.Connect(GameSignals.OnDisplayDialog, DisplayDialog);
		_displayStartDialogConnection = GameSignals.Connect(GameSignals.OnDisplayStartDialog, DisplayStartDialog);
		_displayOkDialogConnection = GameSignals.Connect(GameSignals.OnDisplayOkDialog, DisplayOkDialog);
		_displayGenericDialogConnection = GameSignals.Connect(GameSignals.OnDisplayGenericDialog, DisplayGenericDialog);
		_displayDialogRadioSelectConnection = GameSignals.Connect(GameSignals.OnDisplayDialogRadioSelect, RadioSelect);
		_displayDialogFlag = GameSignals.Connect(GameSignals.OnDisplayDialogFlag, LoginFlag);
		_displayDialogUserName = GameSignals.Connect(GameSignals.OnDisplayDialogUserName, SetLoginUserName);
		_displayDialogPassName = GameSignals.Connect(GameSignals.OnDisplayDialogPassName, SetLoginPassName);
		_closeDialog = GameSignals.Connect(GameSignals.OnCloseDialog, CloseDialog);
		connected = true;
		ConnectedDialog = this;
	}

	private void TryDisconnect()
	{
		if (connected)
		{
			Disconnect();
		}
	}

	private void Disconnect()
	{
		_displayStartDialogConnection.Disconnect();
		_displayDialogConnection.Disconnect();
		_displayOkDialogConnection.Disconnect();
		_displayGenericDialogConnection.Disconnect();
		_displayDialogRadioSelectConnection.Disconnect();
		_displayDialogFlag.Disconnect();
		_displayDialogUserName.Disconnect();
		_displayDialogPassName.Disconnect();
		_closeDialog.Disconnect();
		connected = false;
		ConnectedDialog = null;
	}

	private void CloseDialog()
	{
		mainPanel.DisableActivation();
	}

	private void DisplayGenericDialog(GenericDialog genericDialog)
	{
		mainPanel.EnableActivation();
		TextureGenericDialog textureGenericDialog = genericDialog as TextureGenericDialog;
		SpriteGenericDialog spriteGenericDialog = genericDialog as SpriteGenericDialog;
		LoginGenericDialog loginGenericDialog = genericDialog as LoginGenericDialog;
		InputGenericDialog inputGenericDialog = genericDialog as InputGenericDialog;
		RadioGenericDialog radioDialog = genericDialog as RadioGenericDialog;
		StartDialog startDialog = genericDialog as StartDialog;
		bool active = genericDialog != null;
		bool flag = textureGenericDialog != null;
		bool flag2 = spriteGenericDialog != null;
		bool flag3 = loginGenericDialog != null;
		bool flag4 = inputGenericDialog != null;
		bool flag5 = radioDialog != null;
		bool flag6 = startDialog != null;
		if (uiTitleText != null)
		{
			uiTitleText.gameObject.SetActive(active);
			string title = genericDialog._title;
			title = ((!title.StartsWith("##")) ? title : I2Helper.TryGet(title));
			title = title.ToUpper();
			uiTitleText.text = title;
		}
		if (uiContentText != null)
		{
			uiContentText.gameObject.SetActive(active);
			string content = genericDialog._content;
			content = ((!content.StartsWith("##")) ? content : I2Helper.TryGet(content));
			uiContentText.text = content;
		}
		if (uiContentRawImage != null)
		{
			uiContentRawImage.texture = null;
			uiContentRawImage.texture = (flag ? textureGenericDialog._texture : uiContentRawImage.texture);
			uiContentRawImage.texture = (flag4 ? inputGenericDialog._texture : uiContentRawImage.texture);
			uiContentRawImage.gameObject.SetActive((flag || flag4) && uiContentRawImage.texture != null);
		}
		if (uiContentImage != null)
		{
			uiContentImage.gameObject.SetActive(flag2);
			uiContentImage.sprite = (flag2 ? spriteGenericDialog._sprite : null);
			uiContentImage.SetNativeSize();
		}
		if (uiChanNameInputField != null)
		{
			uiChanNameInputField.gameObject.SetActive(flag3);
			SetLoginChanName(flag3 ? loginGenericDialog._defaultChanName : string.Empty);
		}
		if (uiUserNameInputField != null)
		{
			uiUserNameInputField.gameObject.SetActive(flag3);
			SetLoginUserName(flag3 ? loginGenericDialog._defaultUserName : string.Empty);
		}
		if (uiPassNameInputField != null)
		{
			uiPassNameInputField.gameObject.SetActive(flag3);
			SetLoginPassName(flag3 ? loginGenericDialog._defaultPassName : string.Empty);
		}
		if (uiChanNameEraseButton != null)
		{
			uiChanNameEraseButton.gameObject.SetActive(flag3);
			uiChanNameEraseButton.onClick.RemoveAllListeners();
			if (flag3)
			{
				uiChanNameEraseButton.onClick.AddListener(delegate
				{
					if (loginGenericDialog._chanNameValidation != null)
					{
						SetLoginChanName(string.Empty);
						loginGenericDialog._chanNameValidation(string.Empty);
					}
				});
			}
		}
		if (uiUserNameEraseButton != null)
		{
			uiUserNameEraseButton.gameObject.SetActive(flag3);
			uiUserNameEraseButton.onClick.RemoveAllListeners();
			if (flag3)
			{
				uiUserNameEraseButton.onClick.AddListener(delegate
				{
					if (loginGenericDialog._userNameValidation != null)
					{
						SetLoginUserName(string.Empty);
						loginGenericDialog._userNameValidation(string.Empty);
					}
				});
			}
		}
		if (uiPassNameEraseButton != null)
		{
			uiPassNameEraseButton.gameObject.SetActive(flag3);
			uiPassNameEraseButton.onClick.RemoveAllListeners();
			if (flag3)
			{
				uiPassNameEraseButton.onClick.AddListener(delegate
				{
					if (loginGenericDialog._passNameValidation != null)
					{
						SetLoginPassName(string.Empty);
						loginGenericDialog._passNameValidation(string.Empty);
					}
				});
			}
		}
		if (uiContentInputField != null)
		{
			uiContentInputField.gameObject.SetActive(flag4);
			uiContentInputField.text = (flag4 ? inputGenericDialog._defaultInput : string.Empty);
		}
		if (radioContainee != null)
		{
			radioContainee.ToggleActivation(flag5);
			if (radioContainer != null && flag5)
			{
				radioContainer.ExecActivationAction(radioDialog._selectedIndex, Container.Item.ActivationAction.Activate);
			}
		}
		DisableActivation(radioBtnClones);
		if (flag5)
		{
			for (int num = 0; num < radioDialog._radioBtnLabels.Length; num++)
			{
				ContaineeExtended containeeExtended = radioBtnClones[num];
				containeeExtended.EnableActivation();
				containeeExtended.ResetPlugCache();
				Text plugComponent = containeeExtended.GetPlugComponent<Text>("Label");
				Button plugComponent2 = containeeExtended.GetPlugComponent<Button>("Button");
				string term = radioDialog._radioBtnLabels[num];
				int btnIndex = num;
				plugComponent.text = I2Helper.TryGet(term);
				plugComponent2.onClick.RemoveAllListeners();
				plugComponent2.onClick.AddListener(delegate
				{
					RadioSelect(btnIndex);
					radioDialog._inputValidation(btnIndex);
				});
			}
			RadioSelect(radioDialog._selectedIndex);
		}
		DisableActivation(btnClones);
		for (int num2 = 0; num2 < genericDialog._btns.Length; num2++)
		{
			ContaineeExtended containeeExtended2 = btnClones[num2];
			containeeExtended2.EnableActivation();
			containeeExtended2.ResetPlugCache();
			Text plugComponent3 = containeeExtended2.GetPlugComponent<Text>("Label");
			Text plugComponent4 = containeeExtended2.GetPlugComponent<Text>("OtherLabel");
			Button plugComponent5 = containeeExtended2.GetPlugComponent<Button>("Button");
			Action btnAction = genericDialog._btns[num2]._action;
			Action<string> inputValidationAction = (flag4 ? inputGenericDialog._inputValidation : null);
			Action<bool> loginFlagValidationAction = (flag3 ? loginGenericDialog._flagValidation : null);
			Action<string> loginChanNameValidationAction = (flag3 ? loginGenericDialog._chanNameValidation : null);
			Action<string> loginUserNameValidationAction = (flag3 ? loginGenericDialog._userNameValidation : null);
			Action<string> loginPassNameValidationAction = (flag3 ? loginGenericDialog._passNameValidation : null);
			string label = genericDialog._btns[num2]._label;
			plugComponent3.text = I2Helper.TryGet(label).ToUpper();
			if (plugComponent4 != null)
			{
				if (string.IsNullOrEmpty(genericDialog._btns[num2]._otherLabel))
				{
					plugComponent4.text = string.Empty;
				}
				else
				{
					plugComponent4.text = I2Helper.TryGet(genericDialog._btns[num2]._otherLabel);
				}
			}
			plugComponent5.onClick.RemoveAllListeners();
			plugComponent5.onClick.AddListener(delegate
			{
				if (inputValidationAction != null)
				{
					inputValidationAction(uiContentInputField.text);
				}
				if (loginFlagValidationAction != null)
				{
				}
				if (loginChanNameValidationAction != null)
				{
					loginChanNameValidationAction(uiChanNameInputField.text);
				}
				if (loginUserNameValidationAction != null)
				{
					loginUserNameValidationAction(uiUserNameInputField.text);
				}
				if (loginPassNameValidationAction != null)
				{
					loginPassNameValidationAction(uiPassNameInputField.text);
				}
				btnAction();
			});
		}
		uiRadioContentSizeFitter.SetLayoutVertical();
		uiRadioLayoutElementConstraint.Constrain();
		uiSpriteLayoutElementConstraint.Constrain();
		uiTextureLayoutElementConstraint.Constrain();
		uiTextContentSizeFitter.SetLayoutVertical();
		uiTextLayoutElementConstraint.Constrain();
		uiStartCheckboxGo.SetActive(flag6);
		if (flag6)
		{
			uiStartCheckboxImageGo.SetActive(startDialog.CheckboxValueFunc());
			uiStartCheckboxText.text = I2Helper.TryGet(startDialog.CheckboxText).ToUpper();
			uiStartCheckboxButton.onClick.RemoveAllListeners();
			uiStartCheckboxButton.onClick.AddListener(delegate
			{
				startDialog.CheckboxAction();
				uiStartCheckboxImageGo.SetActive(startDialog.CheckboxValueFunc());
			});
		}
	}

	private void DisplayOkDialog(string titleText = "", string contentText = "", string buttonText = "", Action action = null)
	{
		DisplayGenericDialog(new GenericDialog(titleText, contentText, new GenericDialog.Btn[1]
		{
			new GenericDialog.Btn(buttonText, delegate
			{
				action();
				CloseDialog();
			}, string.Empty)
		}));
	}

	private void DisplayStartDialog(GenericDialog dialog)
	{
		DisplayGenericDialog(dialog);
	}

	private void DisplayDialog(string titleText = "", string contentText = "", string[] buttonTexts = null, Action[] actions = null)
	{
		DisplayGenericDialog(new GenericDialog(titleText, contentText, buttonTexts.Select((string t, int i) => new GenericDialog.Btn(t, delegate
		{
			actions[i]();
		}, string.Empty)).ToArray()));
	}

	private static void DisableActivation(IEnumerable<ContaineeExtended> targets)
	{
		foreach (ContaineeExtended target in targets)
		{
			target.DisableActivation();
		}
	}

	private void LoginFlag(bool dstFlag)
	{
	}

	private void SetLoginChanName(string dstChanName)
	{
		uiChanNameInputField.text = dstChanName;
	}

	private void SetLoginUserName(string dstUserName)
	{
		uiUserNameInputField.text = dstUserName;
	}

	private void SetLoginPassName(string dstPassName)
	{
		uiPassNameInputField.text = dstPassName;
	}

	private void RadioSelect(int idx)
	{
		for (int i = 0; i < radioBtnClones.Count; i++)
		{
			ContaineeExtended containeeExtended = radioBtnClones[i];
			containeeExtended.ResetPlugCache();
			Image plugComponent = containeeExtended.GetPlugComponent<Image>("Check");
			plugComponent.enabled = i == idx;
		}
	}
}
