using System;
using Brix.Shell;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.RuntimeShell;

public class CodeInput : MonoBehaviour
{
	public Button uiLanguagePreviousBtn;

	public Button uiLanguageNextBtn;

	public Text uiLanguageText;

	public Button uiRecallPreviousBtn;

	public Button uiRecallNextBtn;

	public Button uiSubmitBtn;

	public InputField uiInputField;

	public Text uiText;

	public CodeEvaluator evaluator;

	public History history;

	public int languageIdx;

	private string language
	{
		get
		{
			return evaluator.languages[languageIdx].language;
		}
		set
		{
			languageIdx = Array.FindIndex(evaluator.languages, (CodeEvaluator.LanguageBinding binding) => binding.language == value);
			languageIdx = ((languageIdx != -1) ? languageIdx : 0);
		}
	}

	private string input
	{
		get
		{
			return uiInputField.text;
		}
		set
		{
			uiInputField.text = value;
		}
	}

	private void Refresh()
	{
		if (uiLanguageText != null)
		{
			uiLanguageText.text = language;
		}
		if (uiInputField != null)
		{
			uiInputField.text = input;
		}
		if (uiText != null)
		{
			uiText.text = input;
		}
	}

	private void OnEnable()
	{
		if (uiLanguagePreviousBtn != null)
		{
			uiLanguagePreviousBtn.onClick.AddListener(LanguagePrevious);
		}
		if (uiLanguageNextBtn != null)
		{
			uiLanguageNextBtn.onClick.AddListener(LanguageNext);
		}
		if (uiRecallPreviousBtn != null)
		{
			uiRecallPreviousBtn.onClick.AddListener(RecallPrevious);
		}
		if (uiRecallNextBtn != null)
		{
			uiRecallNextBtn.onClick.AddListener(RecallNext);
		}
		if (uiSubmitBtn != null)
		{
			uiSubmitBtn.onClick.AddListener(Submit);
		}
		Refresh();
	}

	private void OnDisable()
	{
		if (uiLanguagePreviousBtn != null)
		{
			uiLanguagePreviousBtn.onClick.RemoveListener(LanguagePrevious);
		}
		if (uiLanguageNextBtn != null)
		{
			uiLanguageNextBtn.onClick.RemoveListener(LanguageNext);
		}
		if (uiRecallPreviousBtn != null)
		{
			uiRecallPreviousBtn.onClick.RemoveListener(RecallPrevious);
		}
		if (uiRecallNextBtn != null)
		{
			uiRecallNextBtn.onClick.RemoveListener(RecallNext);
		}
		if (uiSubmitBtn != null)
		{
			uiSubmitBtn.onClick.RemoveListener(Submit);
		}
	}

	public void Submit()
	{
		Submit(input);
	}

	public void Submit(string s)
	{
		if (!(s == string.Empty) && !(input == string.Empty))
		{
			RecordCurrent();
			evaluator.SendEvalRequest(input, language);
			Refresh();
		}
	}

	public void LanguagePrevious()
	{
		languageIdx--;
		languageIdx = ((languageIdx == -1) ? (evaluator.languages.Length - 1) : languageIdx);
		Refresh();
	}

	public void LanguageNext()
	{
		languageIdx++;
		languageIdx = ((languageIdx < evaluator.languages.Length) ? languageIdx : 0);
		Refresh();
	}

	public void RecordCurrent()
	{
		history.RecordCurrent(language, input.Trim());
	}

	public void RecallPrevious()
	{
		Command command = history.RecallPrevious(language, input.Trim());
		language = command.language;
		input = command.trimmedCode;
		Refresh();
	}

	public void RecallNext()
	{
		Command command = history.RecallNext(language, input.Trim());
		language = command.language;
		input = command.trimmedCode;
		Refresh();
	}
}
