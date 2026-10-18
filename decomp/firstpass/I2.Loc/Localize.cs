using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc;

[AddComponentMenu("I2/Localization/Localize")]
public class Localize : MonoBehaviour
{
	public enum TermModification
	{
		DontModify,
		ToUpper,
		ToLower,
		ToUpperFirst,
		ToTitle
	}

	public delegate void DelegateSetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm);

	public delegate void DelegateDoLocalize(string primaryTerm, string secondaryTerm);

	public string mTerm = string.Empty;

	public string mTermSecondary = string.Empty;

	[NonSerialized]
	public string FinalTerm;

	[NonSerialized]
	public string FinalSecondaryTerm;

	public TermModification PrimaryTermModifier;

	public TermModification SecondaryTermModifier;

	public bool LocalizeOnAwake = true;

	private string LastLocalizedLanguage;

	public UnityEngine.Object mTarget;

	public DelegateSetFinalTerms EventSetFinalTerms;

	public DelegateDoLocalize EventDoLocalize;

	public bool CanUseSecondaryTerm;

	public bool AllowMainTermToBeRTL;

	public bool AllowSecondTermToBeRTL;

	public bool IgnoreRTL;

	public int MaxCharactersInRTL;

	public bool IgnoreNumbersInRTL;

	public bool CorrectAlignmentForRTL = true;

	public UnityEngine.Object[] TranslatedObjects;

	public EventCallback LocalizeCallBack = new EventCallback();

	public static string MainTranslation;

	public static string SecondaryTranslation;

	public static string CallBackTerm;

	public static string CallBackSecondaryTerm;

	public static Localize CurrentLocalizeComponent;

	public bool AlwaysForceLocalize;

	public bool mGUI_ShowReferences;

	public bool mGUI_ShowTems = true;

	public bool mGUI_ShowCallback;

	private Text mTarget_uGUI_Text;

	private Image mTarget_uGUI_Image;

	private RawImage mTarget_uGUI_RawImage;

	private TextAnchor mOriginalAlignmentUGUI;

	private GUIText mTarget_GUIText;

	private TextMesh mTarget_TextMesh;

	private AudioSource mTarget_AudioSource;

	private GUITexture mTarget_GUITexture;

	private GameObject mTarget_Child;

	private SpriteRenderer mTarget_SpriteRenderer;

	private bool mInitializeAlignment = true;

	private TextAlignment mOriginalAlignmentStd;

	public string Term
	{
		get
		{
			return mTerm;
		}
		set
		{
			SetTerm(value);
		}
	}

	public string SecondaryTerm
	{
		get
		{
			return mTermSecondary;
		}
		set
		{
			SetTerm(null, value);
		}
	}

	public event Action EventFindTarget;

	private void Awake()
	{
		RegisterTargets();
		if (HasTargetCache())
		{
			this.EventFindTarget();
		}
		if (LocalizeOnAwake)
		{
			OnLocalize();
		}
	}

	private void RegisterTargets()
	{
		if (this.EventFindTarget == null)
		{
			RegisterEvents_NGUI();
			RegisterEvents_DFGUI();
			RegisterEvents_UGUI();
			RegisterEvents_2DToolKit();
			RegisterEvents_TextMeshPro();
			RegisterEvents_UnityStandard();
			RegisterEvents_SVG();
		}
	}

	private void OnEnable()
	{
		OnLocalize();
	}

	public void OnLocalize(bool Force = false)
	{
		if ((!Force && (!base.enabled || base.gameObject == null || !base.gameObject.activeInHierarchy)) || string.IsNullOrEmpty(LocalizationManager.CurrentLanguage) || (!AlwaysForceLocalize && !Force && !LocalizeCallBack.HasCallback() && LastLocalizedLanguage == LocalizationManager.CurrentLanguage))
		{
			return;
		}
		LastLocalizedLanguage = LocalizationManager.CurrentLanguage;
		if (!HasTargetCache())
		{
			FindTarget();
		}
		if (!HasTargetCache())
		{
			return;
		}
		if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
		{
			GetFinalTerms(out FinalTerm, out FinalSecondaryTerm);
		}
		bool flag = Application.isPlaying && LocalizeCallBack.HasCallback();
		if (!flag && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(FinalSecondaryTerm))
		{
			return;
		}
		CallBackTerm = FinalTerm;
		CallBackSecondaryTerm = FinalSecondaryTerm;
		MainTranslation = LocalizationManager.GetTermTranslation(FinalTerm, FixForRTL: false);
		SecondaryTranslation = LocalizationManager.GetTermTranslation(FinalSecondaryTerm, FixForRTL: false);
		if (!flag && string.IsNullOrEmpty(MainTranslation) && string.IsNullOrEmpty(SecondaryTranslation))
		{
			return;
		}
		CurrentLocalizeComponent = this;
		if (Application.isPlaying)
		{
			LocalizeCallBack.Execute(this);
			LocalizationManager.ApplyLocalizationParams(ref MainTranslation, this);
		}
		if (LocalizationManager.IsRight2Left && !IgnoreRTL)
		{
			if (AllowMainTermToBeRTL && !string.IsNullOrEmpty(MainTranslation))
			{
				MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
			}
			if (AllowSecondTermToBeRTL && !string.IsNullOrEmpty(SecondaryTranslation))
			{
				SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
			}
		}
		switch (PrimaryTermModifier)
		{
		case TermModification.ToUpper:
			MainTranslation = MainTranslation.ToUpper();
			break;
		case TermModification.ToLower:
			MainTranslation = MainTranslation.ToLower();
			break;
		case TermModification.ToUpperFirst:
			MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation);
			break;
		case TermModification.ToTitle:
			MainTranslation = GoogleTranslation.TitleCase(MainTranslation);
			break;
		}
		switch (SecondaryTermModifier)
		{
		case TermModification.ToUpper:
			SecondaryTranslation = SecondaryTranslation.ToUpper();
			break;
		case TermModification.ToLower:
			SecondaryTranslation = SecondaryTranslation.ToLower();
			break;
		case TermModification.ToUpperFirst:
			SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation);
			break;
		case TermModification.ToTitle:
			SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation);
			break;
		}
		EventDoLocalize(MainTranslation, SecondaryTranslation);
		CurrentLocalizeComponent = null;
	}

	public bool FindTarget()
	{
		if (HasTargetCache())
		{
			return true;
		}
		if (this.EventFindTarget == null)
		{
			RegisterTargets();
		}
		this.EventFindTarget();
		return HasTargetCache();
	}

	public void FindAndCacheTarget<T>(ref T targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL) where T : Component
	{
		if (mTarget != null)
		{
			targetCache = mTarget as T;
		}
		else
		{
			mTarget = (targetCache = GetComponent<T>());
		}
		if (targetCache != null)
		{
			EventSetFinalTerms = setFinalTerms;
			EventDoLocalize = doLocalize;
			CanUseSecondaryTerm = UseSecondaryTerm;
			AllowMainTermToBeRTL = MainRTL;
			AllowSecondTermToBeRTL = SecondRTL;
		}
	}

	private void FindAndCacheTarget(ref GameObject targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL)
	{
		if (mTarget != targetCache && (bool)targetCache)
		{
			UnityEngine.Object.Destroy(targetCache);
		}
		if (mTarget != null)
		{
			targetCache = mTarget as GameObject;
		}
		else
		{
			Transform transform = base.transform;
			mTarget = (targetCache = ((transform.childCount >= 1) ? transform.GetChild(0).gameObject : null));
		}
		if (targetCache != null)
		{
			EventSetFinalTerms = setFinalTerms;
			EventDoLocalize = doLocalize;
			CanUseSecondaryTerm = UseSecondaryTerm;
			AllowMainTermToBeRTL = MainRTL;
			AllowSecondTermToBeRTL = SecondRTL;
		}
	}

	private bool HasTargetCache()
	{
		return EventDoLocalize != null;
	}

	public void GetFinalTerms(out string PrimaryTerm, out string SecondaryTerm)
	{
		if (EventSetFinalTerms == null || (!mTarget && !HasTargetCache()))
		{
			FindTarget();
		}
		PrimaryTerm = string.Empty;
		SecondaryTerm = string.Empty;
		if (mTarget != null && (string.IsNullOrEmpty(mTerm) || string.IsNullOrEmpty(mTermSecondary)) && EventSetFinalTerms != null)
		{
			EventSetFinalTerms(mTerm, mTermSecondary, out PrimaryTerm, out SecondaryTerm);
		}
		if (!string.IsNullOrEmpty(mTerm))
		{
			PrimaryTerm = mTerm;
		}
		if (!string.IsNullOrEmpty(mTermSecondary))
		{
			SecondaryTerm = mTermSecondary;
		}
		if (PrimaryTerm != null)
		{
			PrimaryTerm = PrimaryTerm.Trim();
		}
		if (SecondaryTerm != null)
		{
			SecondaryTerm = SecondaryTerm.Trim();
		}
	}

	public string GetMainTargetsText()
	{
		string primaryTerm = null;
		string secondaryTerm = null;
		if (EventSetFinalTerms != null)
		{
			EventSetFinalTerms(null, null, out primaryTerm, out secondaryTerm);
		}
		return (!string.IsNullOrEmpty(primaryTerm)) ? primaryTerm : mTerm;
	}

	private void SetFinalTerms(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm, bool RemoveNonASCII)
	{
		PrimaryTerm = ((!RemoveNonASCII || string.IsNullOrEmpty(Main)) ? Main : Regex.Replace(Main, "[^a-zA-Z0-9_ ]+", " "));
		SecondaryTerm = Secondary;
	}

	public void SetTerm(string primary, string secondary = null)
	{
		if (!string.IsNullOrEmpty(primary))
		{
			FinalTerm = (mTerm = primary);
		}
		if (!string.IsNullOrEmpty(secondary))
		{
			FinalSecondaryTerm = (mTermSecondary = secondary);
		}
		OnLocalize(Force: true);
	}

	private T GetSecondaryTranslatedObj<T>(ref string MainTranslation, ref string SecondaryTranslation) where T : UnityEngine.Object
	{
		DeserializeTranslation(MainTranslation, out var value, out var secondary);
		T val = (T)null;
		if (!string.IsNullOrEmpty(secondary))
		{
			val = GetObject<T>(secondary);
			if (val != null)
			{
				MainTranslation = value;
				SecondaryTranslation = secondary;
			}
		}
		if (val == null)
		{
			val = GetObject<T>(SecondaryTranslation);
		}
		return val;
	}

	private T GetObject<T>(string Translation) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(Translation))
		{
			return (T)null;
		}
		T translatedObject = GetTranslatedObject<T>(Translation);
		if (translatedObject == null)
		{
			int num = Translation.LastIndexOfAny("/\\".ToCharArray());
			if (num >= 0)
			{
				Translation = Translation.Substring(num + 1);
				translatedObject = GetTranslatedObject<T>(Translation);
			}
		}
		return translatedObject;
	}

	private T GetTranslatedObject<T>(string Translation) where T : UnityEngine.Object
	{
		return FindTranslatedObject<T>(Translation);
	}

	private void DeserializeTranslation(string translation, out string value, out string secondary)
	{
		if (!string.IsNullOrEmpty(translation) && translation.Length > 1 && translation[0] == '[')
		{
			int num = translation.IndexOf(']');
			if (num > 0)
			{
				secondary = translation.Substring(1, num - 1);
				value = translation.Substring(num + 1);
				return;
			}
		}
		value = translation;
		secondary = string.Empty;
	}

	public T FindTranslatedObject<T>(string value) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(value))
		{
			return (T)null;
		}
		if (TranslatedObjects != null)
		{
			int i = 0;
			for (int num = TranslatedObjects.Length; i < num; i++)
			{
				if (TranslatedObjects[i] as T != null && value == TranslatedObjects[i].name)
				{
					return TranslatedObjects[i] as T;
				}
			}
		}
		T val = LocalizationManager.FindAsset(value) as T;
		if ((bool)val)
		{
			return val;
		}
		return ResourceManager.pInstance.GetAsset<T>(value);
	}

	public bool HasTranslatedObject(UnityEngine.Object Obj)
	{
		if (Array.IndexOf(TranslatedObjects, Obj) >= 0)
		{
			return true;
		}
		return ResourceManager.pInstance.HasAsset(Obj);
	}

	public void AddTranslatedObject(UnityEngine.Object Obj)
	{
		Array.Resize(ref TranslatedObjects, TranslatedObjects.Length + 1);
		TranslatedObjects[TranslatedObjects.Length - 1] = Obj;
	}

	public void SetGlobalLanguage(string Language)
	{
		LocalizationManager.CurrentLanguage = Language;
	}

	public static void RegisterEvents_2DToolKit()
	{
	}

	public static void RegisterEvents_DFGUI()
	{
	}

	public static void RegisterEvents_NGUI()
	{
	}

	public static void RegisterEvents_SVG()
	{
	}

	public static void RegisterEvents_TextMeshPro()
	{
	}

	public void RegisterEvents_UGUI()
	{
		EventFindTarget += FindTarget_uGUI_Text;
		EventFindTarget += FindTarget_uGUI_Image;
		EventFindTarget += FindTarget_uGUI_RawImage;
	}

	private void FindTarget_uGUI_Text()
	{
		FindAndCacheTarget(ref mTarget_uGUI_Text, SetFinalTerms_uGUI_Text, DoLocalize_uGUI_Text, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
	}

	private void FindTarget_uGUI_Image()
	{
		FindAndCacheTarget(ref mTarget_uGUI_Image, SetFinalTerms_uGUI_Image, DoLocalize_uGUI_Image, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	private void FindTarget_uGUI_RawImage()
	{
		FindAndCacheTarget(ref mTarget_uGUI_RawImage, SetFinalTerms_uGUI_RawImage, DoLocalize_uGUI_RawImage, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	private void SetFinalTerms_uGUI_Text(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		string secondary = ((!(mTarget_uGUI_Text.font != null)) ? string.Empty : mTarget_uGUI_Text.font.name);
		SetFinalTerms(mTarget_uGUI_Text.text, secondary, out primaryTerm, out secondaryTerm, RemoveNonASCII: true);
	}

	public void SetFinalTerms_uGUI_Image(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		SetFinalTerms(mTarget_uGUI_Image.mainTexture.name, null, out primaryTerm, out secondaryTerm, RemoveNonASCII: false);
	}

	public void SetFinalTerms_uGUI_RawImage(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		SetFinalTerms(mTarget_uGUI_RawImage.texture.name, null, out primaryTerm, out secondaryTerm, RemoveNonASCII: false);
	}

	public static T FindInParents<T>(Transform tr) where T : Component
	{
		if (!tr)
		{
			return (T)null;
		}
		T component = tr.GetComponent<T>();
		while (!component && (bool)tr)
		{
			component = tr.GetComponent<T>();
			tr = tr.parent;
		}
		return component;
	}

	public void DoLocalize_uGUI_Text(string MainTranslation, string SecondaryTranslation)
	{
		Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
		if (secondaryTranslatedObj != null && secondaryTranslatedObj != mTarget_uGUI_Text.font)
		{
			mTarget_uGUI_Text.font = secondaryTranslatedObj;
		}
		if (mInitializeAlignment)
		{
			mInitializeAlignment = false;
			mOriginalAlignmentUGUI = mTarget_uGUI_Text.alignment;
		}
		if (string.IsNullOrEmpty(MainTranslation) || !(mTarget_uGUI_Text.text != MainTranslation))
		{
			return;
		}
		if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
		{
			if (mTarget_uGUI_Text.alignment == TextAnchor.UpperLeft || mTarget_uGUI_Text.alignment == TextAnchor.UpperRight)
			{
				mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? mOriginalAlignmentUGUI : TextAnchor.UpperRight);
			}
			else if (mTarget_uGUI_Text.alignment == TextAnchor.MiddleLeft || mTarget_uGUI_Text.alignment == TextAnchor.MiddleRight)
			{
				mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? mOriginalAlignmentUGUI : TextAnchor.MiddleRight);
			}
			else if (mTarget_uGUI_Text.alignment == TextAnchor.LowerLeft || mTarget_uGUI_Text.alignment == TextAnchor.LowerRight)
			{
				mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? mOriginalAlignmentUGUI : TextAnchor.LowerRight);
			}
		}
		mTarget_uGUI_Text.text = MainTranslation;
		mTarget_uGUI_Text.SetVerticesDirty();
	}

	public void DoLocalize_uGUI_Image(string MainTranslation, string SecondaryTranslation)
	{
		Sprite sprite = mTarget_uGUI_Image.sprite;
		if (sprite == null || sprite.name != MainTranslation)
		{
			mTarget_uGUI_Image.sprite = FindTranslatedObject<Sprite>(MainTranslation);
		}
	}

	public void DoLocalize_uGUI_RawImage(string MainTranslation, string SecondaryTranslation)
	{
		Texture texture = mTarget_uGUI_RawImage.texture;
		if (texture == null || texture.name != MainTranslation)
		{
			mTarget_uGUI_RawImage.texture = FindTranslatedObject<Texture>(MainTranslation);
		}
	}

	public void RegisterEvents_UnityStandard()
	{
		EventFindTarget += FindTarget_GUIText;
		EventFindTarget += FindTarget_TextMesh;
		EventFindTarget += FindTarget_AudioSource;
		EventFindTarget += FindTarget_GUITexture;
		EventFindTarget += FindTarget_Child;
		EventFindTarget += FindTarget_SpriteRenderer;
	}

	private void FindTarget_GUIText()
	{
		FindAndCacheTarget(ref mTarget_GUIText, SetFinalTerms_GUIText, DoLocalize_GUIText, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
	}

	private void FindTarget_TextMesh()
	{
		FindAndCacheTarget(ref mTarget_TextMesh, SetFinalTerms_TextMesh, DoLocalize_TextMesh, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
	}

	private void FindTarget_AudioSource()
	{
		FindAndCacheTarget(ref mTarget_AudioSource, SetFinalTerms_AudioSource, DoLocalize_AudioSource, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	private void FindTarget_GUITexture()
	{
		FindAndCacheTarget(ref mTarget_GUITexture, SetFinalTerms_GUITexture, DoLocalize_GUITexture, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	private void FindTarget_Child()
	{
		FindAndCacheTarget(ref mTarget_Child, SetFinalTerms_Child, DoLocalize_Child, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	private void FindTarget_SpriteRenderer()
	{
		FindAndCacheTarget(ref mTarget_SpriteRenderer, SetFinalTerms_SpriteRenderer, DoLocalize_SpriteRenderer, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
	}

	public void SetFinalTerms_GUIText(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		if (string.IsNullOrEmpty(Secondary) && mTarget_GUIText.font != null)
		{
			Secondary = mTarget_GUIText.font.name;
		}
		SetFinalTerms(mTarget_GUIText.text, Secondary, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: true);
	}

	public void SetFinalTerms_TextMesh(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		string secondary = ((!(mTarget_TextMesh.font != null)) ? string.Empty : mTarget_TextMesh.font.name);
		SetFinalTerms(mTarget_TextMesh.text, secondary, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: true);
	}

	public void SetFinalTerms_GUITexture(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		if (!mTarget_GUITexture || !mTarget_GUITexture.texture)
		{
			SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
		}
		else
		{
			SetFinalTerms(mTarget_GUITexture.texture.name, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
		}
	}

	public void SetFinalTerms_AudioSource(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		if (!mTarget_AudioSource || !mTarget_AudioSource.clip)
		{
			SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
		}
		else
		{
			SetFinalTerms(mTarget_AudioSource.clip.name, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
		}
	}

	public void SetFinalTerms_Child(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		SetFinalTerms(mTarget_Child.name, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
	}

	public void SetFinalTerms_SpriteRenderer(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
	{
		SetFinalTerms((!(mTarget_SpriteRenderer.sprite != null)) ? string.Empty : mTarget_SpriteRenderer.sprite.name, string.Empty, out PrimaryTerm, out SecondaryTerm, RemoveNonASCII: false);
	}

	private void DoLocalize_GUIText(string MainTranslation, string SecondaryTranslation)
	{
		Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
		if (secondaryTranslatedObj != null && mTarget_GUIText.font != secondaryTranslatedObj)
		{
			mTarget_GUIText.font = secondaryTranslatedObj;
		}
		if (mInitializeAlignment)
		{
			mInitializeAlignment = false;
			mOriginalAlignmentStd = mTarget_GUIText.alignment;
		}
		if (!string.IsNullOrEmpty(MainTranslation) && mTarget_GUIText.text != MainTranslation)
		{
			if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_GUIText.alignment != TextAlignment.Center)
			{
				mTarget_GUIText.alignment = ((!LocalizationManager.IsRight2Left) ? mOriginalAlignmentStd : TextAlignment.Right);
			}
			mTarget_GUIText.text = MainTranslation;
		}
	}

	private void DoLocalize_TextMesh(string MainTranslation, string SecondaryTranslation)
	{
		Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
		if (secondaryTranslatedObj != null && mTarget_TextMesh.font != secondaryTranslatedObj)
		{
			mTarget_TextMesh.font = secondaryTranslatedObj;
			GetComponent<Renderer>().sharedMaterial = secondaryTranslatedObj.material;
		}
		if (mInitializeAlignment)
		{
			mInitializeAlignment = false;
			mOriginalAlignmentStd = mTarget_TextMesh.alignment;
		}
		if (!string.IsNullOrEmpty(MainTranslation) && mTarget_TextMesh.text != MainTranslation)
		{
			if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_TextMesh.alignment != TextAlignment.Center)
			{
				mTarget_TextMesh.alignment = ((!LocalizationManager.IsRight2Left) ? mOriginalAlignmentStd : TextAlignment.Right);
			}
			mTarget_TextMesh.text = MainTranslation;
		}
	}

	private void DoLocalize_AudioSource(string MainTranslation, string SecondaryTranslation)
	{
		bool isPlaying = mTarget_AudioSource.isPlaying;
		AudioClip clip = mTarget_AudioSource.clip;
		AudioClip audioClip = FindTranslatedObject<AudioClip>(MainTranslation);
		if (clip != audioClip)
		{
			mTarget_AudioSource.clip = audioClip;
		}
		if (isPlaying && (bool)mTarget_AudioSource.clip)
		{
			mTarget_AudioSource.Play();
		}
	}

	private void DoLocalize_GUITexture(string MainTranslation, string SecondaryTranslation)
	{
		Texture texture = mTarget_GUITexture.texture;
		if (texture != null && texture.name != MainTranslation)
		{
			mTarget_GUITexture.texture = FindTranslatedObject<Texture>(MainTranslation);
		}
	}

	private void DoLocalize_Child(string MainTranslation, string SecondaryTranslation)
	{
		if (!mTarget_Child || !(mTarget_Child.name == MainTranslation))
		{
			GameObject gameObject = mTarget_Child;
			GameObject gameObject2 = FindTranslatedObject<GameObject>(MainTranslation);
			if ((bool)gameObject2)
			{
				mTarget_Child = UnityEngine.Object.Instantiate(gameObject2);
				Transform transform = mTarget_Child.transform;
				Transform transform2 = ((!gameObject) ? gameObject2.transform : gameObject.transform);
				transform.parent = base.transform;
				transform.localScale = transform2.localScale;
				transform.localRotation = transform2.localRotation;
				transform.localPosition = transform2.localPosition;
			}
			if ((bool)gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	private void DoLocalize_SpriteRenderer(string MainTranslation, string SecondaryTranslation)
	{
		Sprite sprite = mTarget_SpriteRenderer.sprite;
		if (sprite == null || sprite.name != MainTranslation)
		{
			mTarget_SpriteRenderer.sprite = FindTranslatedObject<Sprite>(MainTranslation);
		}
	}
}
