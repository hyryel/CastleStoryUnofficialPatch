using System.Collections;
using UnityEngine.SceneManagement;

public class GA_Design
{
	public void NewEvent(string eventName, float? eventValue)
	{
		CreateNewEvent(eventName, eventValue);
	}

	public void NewEvent(string eventName)
	{
		CreateNewEvent(eventName, null);
	}

	private void CreateNewEvent(string eventName, float? eventValue)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.EventID], eventName);
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Level], (!GA.SettingsGA.CustomArea.Equals(string.Empty)) ? GA.SettingsGA.CustomArea : SceneManager.GetActiveScene().name);
		Hashtable hashtable2 = hashtable;
		if (eventValue.HasValue)
		{
			hashtable2.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Value], eventValue.ToString());
		}
		GA_Queue.AddItem(hashtable2, GA_Submit.CategoryType.GA_Event, stack: false);
		GA_AdSupport.ShowAdStatic(GA_AdSupport.GAEventType.Custom, GA_AdSupport.GAEventCat.Design, eventName);
	}
}
