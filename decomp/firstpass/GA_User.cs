using System.Collections;

public class GA_User
{
	public enum Gender
	{
		Unknown,
		Male,
		Female
	}

	public void NewUser(Gender gender, int? birth_year, int? friend_count)
	{
		CreateNewUser(gender, birth_year, friend_count, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
	}

	public void NewUser(Gender gender, int? birth_year, int? friend_count, string ios_id, string android_id, string platform, string device, string os, string osVersion, string sdk, string android_adID)
	{
		CreateNewUser(gender, birth_year, friend_count, ios_id, android_id, platform, device, os, osVersion, sdk, null, null, null, null, null, null, null, android_adID);
	}

	public void NewUser(Gender gender, int? birth_year, int? friend_count, string ios_id, string android_id, string platform, string device, string os, string osVersion, string sdk, string installPublisher, string installSite, string installCampaign, string installAdgroup, string installAd, string installKeyword, string facebookID, string android_adID)
	{
		CreateNewUser(gender, birth_year, friend_count, ios_id, android_id, platform, device, os, osVersion, sdk, installPublisher, installSite, installCampaign, installAdgroup, installAd, installKeyword, facebookID, android_adID);
	}

	private void CreateNewUser(Gender gender, int? birth_year, int? friend_count, string ios_id, string android_id, string platform, string device, string os, string osVersion, string sdk, string installPublisher, string installSite, string installCampaign, string installAdgroup, string installAd, string installKeyword, string facebookID, string android_adID)
	{
		Hashtable hashtable = new Hashtable();
		switch (gender)
		{
		case Gender.Male:
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Gender], 'M');
			break;
		case Gender.Female:
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Gender], 'F');
			break;
		}
		if (birth_year.HasValue && birth_year.Value != 0)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Birth_year], birth_year.ToString());
		}
		if (friend_count.HasValue)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Friend_Count], friend_count.ToString());
		}
		if (ios_id != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Ios_id], ios_id);
		}
		if (android_id != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Android_id], android_id);
		}
		if (platform != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Platform], platform);
		}
		if (device != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Device], device);
		}
		if (os != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Os], os);
		}
		if (osVersion != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.OsVersion], osVersion);
		}
		if (sdk != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Sdk], sdk);
		}
		if (installPublisher != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallPublisher], installPublisher);
		}
		if (installSite != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallSite], installSite);
		}
		if (installCampaign != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallCampaign], installCampaign);
		}
		if (installAdgroup != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallAdgroup], installAdgroup);
		}
		if (installAd != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallAd], installAd);
		}
		if (installKeyword != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.InstallKeyword], installKeyword);
		}
		if (facebookID != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.FacebookID], facebookID);
		}
		if (android_adID != null)
		{
			hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.AndroidAdID], android_adID);
		}
		if (hashtable.Count == 0)
		{
			GA.LogWarning("GA: No data to send with NewUser event; event will not be added to queue");
		}
		else
		{
			GA_Queue.AddItem(hashtable, GA_Submit.CategoryType.GA_User, stack: false);
		}
	}
}
