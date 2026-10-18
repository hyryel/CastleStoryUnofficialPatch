using Brix.Game.Network;
using Brix.Utils.Logging;
using Steamworks;

namespace Brix.ThirdParty.Steamworks;

public static class SteamworksPlugin
{
	public abstract class Wrapper<T>
	{
		public T _value;
	}

	public class _Callback<T, T2> : Wrapper<Callback<T2>> where T : Wrapper<T2>, new()
	{
		public delegate void DispatchDelegate(T param);

		public static void Register(DispatchDelegate func, bool bGameServer = false, bool bKeepAlive = true)
		{
			logger.Info("Registering Callback: Steam Enabled");
			new Callback<T2>(delegate(T2 t)
			{
				func(new T
				{
					_value = t
				});
			}, bGameServer);
		}
	}

	public class GetAuthSessionTicketResponse_t : Wrapper<global::Steamworks.GetAuthSessionTicketResponse_t>
	{
		public class Callback : _Callback<GetAuthSessionTicketResponse_t, global::Steamworks.GetAuthSessionTicketResponse_t>
		{
		}
	}

	public class HAuthTicket : Wrapper<global::Steamworks.HAuthTicket>
	{
	}

	public class CSteamID : Wrapper<global::Steamworks.CSteamID>
	{
		public override string ToString()
		{
			return _value.ToString();
		}
	}

	public class EUserHasLicenseForAppResult : Wrapper<global::Steamworks.EUserHasLicenseForAppResult>
	{
		public static EUserHasLicenseForAppResult k_EUserHasLicenseResultDoesNotHaveLicense = new EUserHasLicenseForAppResult
		{
			_value = global::Steamworks.EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense
		};
	}

	public class AppId_t : Wrapper<global::Steamworks.AppId_t>
	{
	}

	private static BrixLogger logger = new BrixLogger(typeof(SteamworksPlugin).FullName);

	public static bool Initialized
	{
		get
		{
			logger.Info("Initializing SteamWork: Steamworks Enabled");
			return SteamManager.Initialized;
		}
	}

	public static void TriggerScreenshot()
	{
		logger.Info("TriggerScreenshot :  Steamworks Enabled");
		SteamScreenshots.TriggerScreenshot();
	}

	public static HAuthTicket GetAuthSessionTicket(byte[] pTicket, int cbMaxTicket, out uint pcbTicket)
	{
		logger.Info("HAuthTicket :  Steamworks Enabled");
		HAuthTicket hAuthTicket = new HAuthTicket();
		hAuthTicket._value = SteamUser.GetAuthSessionTicket(pTicket, cbMaxTicket, out pcbTicket);
		return hAuthTicket;
	}

	public static CSteamID GetSteamID()
	{
		logger.Info("CSteamID :  Steamworks Enabled");
		CSteamID cSteamID = new CSteamID();
		cSteamID._value = SteamUser.GetSteamID();
		return cSteamID;
	}

	public static string GetFriendPersonaName(CSteamID steamIDFriend)
	{
		logger.Info("GetFriend :  Steamworks Enabled");
		return SteamFriends.GetFriendPersonaName(steamIDFriend._value);
	}

	public static AppId_t GetAppID()
	{
		logger.Info("GetAppId :  Steamworks Enabled");
		AppId_t appId_t = new AppId_t();
		appId_t._value = SteamUtils.GetAppID();
		return appId_t;
	}

	public static EUserHasLicenseForAppResult UserHasLicenseForApp(CSteamID steamID, AppId_t appID)
	{
		logger.Info("EUserHasLiscense.. :  Steamworks Enabled");
		EUserHasLicenseForAppResult eUserHasLicenseForAppResult = new EUserHasLicenseForAppResult();
		eUserHasLicenseForAppResult._value = SteamUser.UserHasLicenseForApp(steamID._value, appID._value);
		return eUserHasLicenseForAppResult;
	}
}
