using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class ConnectionGraph2 : PluginInterface2
{
	private HandleRef swigCPtr;

	internal ConnectionGraph2(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.ConnectionGraph2_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public ConnectionGraph2()
		: this(RakNetPINVOKE.new_ConnectionGraph2(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(ConnectionGraph2 obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~ConnectionGraph2()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_ConnectionGraph2(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public bool GetConnectionListForRemoteSystem(RakNetGUID remoteSystemGuid, SystemAddress[] saOut, RakNetGUID[] guidOut, ref uint inOutLength)
	{
		uint num = inOutLength;
		if (guidOut.Length < num)
		{
			num = (uint)guidOut.Length;
		}
		if (saOut.Length < num)
		{
			num = (uint)saOut.Length;
		}
		RakNetListRakNetGUID rakNetListRakNetGUID = new RakNetListRakNetGUID();
		RakNetListSystemAddress rakNetListSystemAddress = new RakNetListSystemAddress();
		bool connectionListForRemoteSystemHelper = GetConnectionListForRemoteSystemHelper(remoteSystemGuid, rakNetListSystemAddress, rakNetListRakNetGUID, ref inOutLength);
		if (inOutLength < num)
		{
			num = inOutLength;
		}
		for (int i = 0; i < num; i++)
		{
			guidOut[i] = rakNetListRakNetGUID[i];
			saOut[i] = rakNetListSystemAddress[i];
		}
		return connectionListForRemoteSystemHelper;
	}

	public void GetParticipantList(RakNetGUID[] participantList)
	{
		RakNetListRakNetGUID rakNetListRakNetGUID = new RakNetListRakNetGUID();
		GetParticipantListHelper(rakNetListRakNetGUID);
		for (int i = 0; i < participantList.Length && i < rakNetListRakNetGUID.Size(); i++)
		{
			participantList[i] = rakNetListRakNetGUID[i];
		}
	}

	public static ConnectionGraph2 GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.ConnectionGraph2_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new ConnectionGraph2(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(ConnectionGraph2 i)
	{
		RakNetPINVOKE.ConnectionGraph2_DestroyInstance(getCPtr(i));
	}

	public bool GetConnectionListForRemoteSystem(RakNetGUID remoteSystemGuid, SystemAddress saOut, RakNetGUID guidOut, out uint outLength)
	{
		bool result = RakNetPINVOKE.ConnectionGraph2_GetConnectionListForRemoteSystem(swigCPtr, RakNetGUID.getCPtr(remoteSystemGuid), SystemAddress.getCPtr(saOut), RakNetGUID.getCPtr(guidOut), out outLength);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ConnectionExists(RakNetGUID g1, RakNetGUID g2)
	{
		bool result = RakNetPINVOKE.ConnectionGraph2_ConnectionExists(swigCPtr, RakNetGUID.getCPtr(g1), RakNetGUID.getCPtr(g2));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ushort GetPingBetweenSystems(RakNetGUID g1, RakNetGUID g2)
	{
		ushort result = RakNetPINVOKE.ConnectionGraph2_GetPingBetweenSystems(swigCPtr, RakNetGUID.getCPtr(g1), RakNetGUID.getCPtr(g2));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakNetGUID GetLowestAveragePingSystem()
	{
		return new RakNetGUID(RakNetPINVOKE.ConnectionGraph2_GetLowestAveragePingSystem(swigCPtr), cMemoryOwn: true);
	}

	public void SetAutoProcessNewConnections(bool b)
	{
		RakNetPINVOKE.ConnectionGraph2_SetAutoProcessNewConnections(swigCPtr, b);
	}

	public bool GetAutoProcessNewConnections()
	{
		return RakNetPINVOKE.ConnectionGraph2_GetAutoProcessNewConnections(swigCPtr);
	}

	public void AddParticipant(SystemAddress systemAddress, RakNetGUID rakNetGUID)
	{
		RakNetPINVOKE.ConnectionGraph2_AddParticipant(swigCPtr, SystemAddress.getCPtr(systemAddress), RakNetGUID.getCPtr(rakNetGUID));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private bool GetConnectionListForRemoteSystemHelper(RakNetGUID remoteSystemGuid, RakNetListSystemAddress saOut, RakNetListRakNetGUID guidOut, ref uint inOutLength)
	{
		bool result = RakNetPINVOKE.ConnectionGraph2_GetConnectionListForRemoteSystemHelper(swigCPtr, RakNetGUID.getCPtr(remoteSystemGuid), RakNetListSystemAddress.getCPtr(saOut), RakNetListRakNetGUID.getCPtr(guidOut), ref inOutLength);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void GetParticipantListHelper(RakNetListRakNetGUID guidOut)
	{
		RakNetPINVOKE.ConnectionGraph2_GetParticipantListHelper(swigCPtr, RakNetListRakNetGUID.getCPtr(guidOut));
	}
}
