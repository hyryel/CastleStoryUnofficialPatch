using System.Collections;
using UnityEngine.SceneManagement;

public class GA_Business
{
	public void NewEvent(string eventName, string currency, int amount)
	{
		CreateNewEvent(eventName, currency, amount);
	}

	private void CreateNewEvent(string eventName, string currency, int amount)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.EventID], eventName);
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Currency], currency);
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Amount], amount.ToString());
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Level], (!GA.SettingsGA.CustomArea.Equals(string.Empty)) ? GA.SettingsGA.CustomArea : SceneManager.GetActiveScene().name);
		Hashtable parameters = hashtable;
		GA_Queue.AddItem(parameters, GA_Submit.CategoryType.GA_Purchase, stack: false);
		GA_AdSupport.ShowAdStatic(GA_AdSupport.GAEventType.Custom, GA_AdSupport.GAEventCat.Business, eventName);
	}
}
