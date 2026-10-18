using System.Collections;
using UnityEngine.SceneManagement;

public class GA_Error
{
	public enum SeverityType
	{
		critical,
		error,
		warning,
		info,
		debug
	}

	public void NewEvent(SeverityType severity, string message)
	{
		CreateNewEvent(severity, message, stack: false);
	}

	public void NewErrorEvent(SeverityType severity, string message)
	{
		CreateNewEvent(severity, message, stack: true);
	}

	private void CreateNewEvent(SeverityType severity, string message, bool stack)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Severity], severity.ToString());
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Message], message);
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Level], (!GA.SettingsGA.CustomArea.Equals(string.Empty)) ? GA.SettingsGA.CustomArea : SceneManager.GetActiveScene().name);
		Hashtable parameters = hashtable;
		GA_Queue.AddItem(parameters, GA_Submit.CategoryType.GA_Error, stack);
	}
}
