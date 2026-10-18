using System;
using System.Collections;
using Brix.External.Signals;
using Brix.Network;
using Brix.ThirdParty.Steamworks;
using Newtonsoft.Json;
using Steamworks;

namespace Brix.Game.Network;

public class GameBackend : AbstractBackend
{
	public static Signal<Hashtable> OnAssociationReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> OnCreateUserReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> OnServersReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> OnServerGamesReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> OnUserInfoReceived = new Signal<Hashtable>();

	public string authTicket;

	private byte[] m_Ticket;

	private SteamworksPlugin.HAuthTicket m_HAuthTicket;

	private SteamManager steamManager;

	private int userId;

	public PlayerBackend playerBackend;

	public bool alwaysKeepMultiplayerEnabled;

	public void GetServerGames()
	{
		SendRequest("game", OnServerGamesReceived);
	}

	public void GetServers()
	{
		SendRequest("server", OnServersReceived);
	}

	public void GetAssociation(string association_user_id)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("association_user_id", association_user_id);
		SendRequest("association", OnAssociationReceived, "GET", hashtable);
	}

	public void GetUserInfo(string username)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("username", username);
		SendRequest("user", OnUserInfoReceived, "GET", hashtable);
	}

	public void CreateUser(string username, string password, string email, string firstName, string lastName)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("username", username);
		hashtable.Add("password", password);
		hashtable.Add("email", email);
		hashtable.Add("first_name", firstName);
		hashtable.Add("last_name", lastName);
		SendRequest("user", OnCreateUserReceived, "POST", hashtable);
	}

	protected override void Init()
	{
		base.Init();
		AbstractBackend.signals.ConnectOnce(AbstractBackend.InternalAuthenticateReceived, base.OnAuthenticateReceived);
		AbstractBackend.signals.ConnectOnce(AbstractBackend.AuthenticateReceived, OnAuthenticationReceived);
		Authenticate(GAME_CLIENT_USERNAME, GAME_CLIENT_PASSWORD);
		SteamworksPlugin._Callback<SteamworksPlugin.GetAuthSessionTicketResponse_t, GetAuthSessionTicketResponse_t>.Register(GetAuthSessionTicketResponse);
		AbstractBackend.signals.Connect(OnAssociationReceived, HandleOnAssociationReceived);
	}

	private void OnAuthenticationReceived(Hashtable data)
	{
		playerBackend = base.gameObject.AddComponent<PlayerBackend>();
		playerBackend.CLIENT_ID = CLIENT_ID;
		playerBackend.CLIENT_SECRET = CLIENT_SECRET;
		playerBackend.BACKEND_URL = BACKEND_URL;
		playerBackend.GAME_CLIENT_USERNAME = string.Empty;
		playerBackend.GAME_CLIENT_PASSWORD = string.Empty;
	}

	private void HandleOnAssociationReceived(Hashtable data)
	{
		if (!data.ContainsKey("objects"))
		{
			throw new Exception("An error occured while getting the association.");
		}
		ArrayList arrayList = JsonConvert.DeserializeObject<ArrayList>(data["objects"].ToString());
		if (IsAccountLinked(arrayList))
		{
			Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(arrayList[0].ToString());
			Hashtable hashtable2 = JsonConvert.DeserializeObject<Hashtable>(hashtable["user"].ToString());
			playerBackend.UserId = int.Parse(hashtable2["id"].ToString());
		}
		m_Ticket = new byte[1024];
		SteamworksPlugin.GetAuthSessionTicket(m_Ticket, 1024, out var _);
	}

	private bool IsAccountLinked(ArrayList accounts)
	{
		return accounts.Count > 0;
	}

	private void GetAuthSessionTicketResponse(SteamworksPlugin.GetAuthSessionTicketResponse_t value)
	{
		if (m_Ticket != null)
		{
			authTicket = ByteToHexBitFiddle(m_Ticket);
			if (playerBackend.UserId != 0)
			{
				AbstractBackend.signals.ConnectOnce(AbstractBackend.AuthorizeUserReceived, OnAuthorizeUserReceived);
				AuthorizeUser(SteamworksPlugin.GetSteamID().ToString(), playerBackend.UserId, authTicket);
			}
		}
	}

	public void OnAuthorizeUserReceived(Hashtable data)
	{
		if (!data.ContainsKey("code"))
		{
			throw new Exception("No Authorization Code Received.");
		}
		AbstractBackend.signals.ConnectOnce(AbstractBackend.InternalAuthenticateReceived, playerBackend.OnAuthenticateReceived);
		playerBackend.Authenticate(data["code"].ToString());
	}

	public string ByteToHexBitFiddle(byte[] bytes)
	{
		char[] array = new char[bytes.Length * 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			int num = bytes[i] >> 4;
			array[i * 2] = (char)(55 + num + ((num - 10 >> 31) & -7));
			num = bytes[i] & 0xF;
			array[i * 2 + 1] = (char)(55 + num + ((num - 10 >> 31) & -7));
		}
		return new string(array);
	}
}
