using System.Collections;
using Brix.External.Signals;
using Brix.Network;
using Newtonsoft.Json;

namespace Brix.Game.Network;

public class PlayerBackend : AbstractBackend
{
	public int UserId;

	public static Signal<Hashtable> LinkThirdPartyAccountReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> UserInfoReceived = new Signal<Hashtable>();

	private static Signal<Hashtable> InternalUserInfoReceived = new Signal<Hashtable>();

	public void GetUserInfo()
	{
		if (UserId != 0)
		{
			SendRequest("user/" + UserId, InternalUserInfoReceived);
		}
		else if (!string.IsNullOrEmpty(base.Username))
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("username", base.Username);
			SendRequest("user", InternalUserInfoReceived, "GET", hashtable);
		}
	}

	public void LinkThirdPartyAccount(int userId, string associationUserId, string associationType, string ticket)
	{
		UserId = userId;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("association_user_id", associationUserId);
		hashtable.Add("association_type", associationType);
		hashtable.Add("ticket", ticket);
		SendRequest("association", LinkThirdPartyAccountReceived, "POST", hashtable);
	}

	protected override void Init()
	{
		base.Init();
		AbstractBackend.signals.Connect(AbstractBackend.AuthenticateReceived, OnAuthenticateReceived);
		AbstractBackend.signals.Connect(InternalUserInfoReceived, OnUserInfoReceived);
	}

	private new void OnAuthenticateReceived(Hashtable data)
	{
		if (!data.ContainsKey("error"))
		{
			GetUserInfo();
		}
	}

	private void OnUserInfoReceived(Hashtable data)
	{
		if (data.ContainsKey("username"))
		{
			base.Username = data["username"].ToString();
		}
		else if (data.ContainsKey("objects"))
		{
			ArrayList arrayList = JsonConvert.DeserializeObject<ArrayList>(data["objects"].ToString());
			if (arrayList.Count > 0)
			{
				Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(arrayList[0].ToString());
				base.Username = (string)hashtable["username"];
			}
		}
		UserInfoReceived.Invoke(data);
	}
}
