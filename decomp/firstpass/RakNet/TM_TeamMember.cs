using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class TM_TeamMember : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal TM_TeamMember(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public TM_TeamMember()
		: this(RakNetPINVOKE.new_TM_TeamMember(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(TM_TeamMember obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~TM_TeamMember()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_TM_TeamMember(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public static TM_TeamMember GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_TeamMember_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new TM_TeamMember(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(TM_TeamMember i)
	{
		RakNetPINVOKE.TM_TeamMember_DestroyInstance(getCPtr(i));
	}

	public bool RequestTeam(TeamSelection teamSelection)
	{
		bool result = RakNetPINVOKE.TM_TeamMember_RequestTeam(swigCPtr, TeamSelection.getCPtr(teamSelection));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RequestTeamSwitch(TM_Team teamToJoin, TM_Team teamToLeave)
	{
		return RakNetPINVOKE.TM_TeamMember_RequestTeamSwitch(swigCPtr, TM_Team.getCPtr(teamToJoin), TM_Team.getCPtr(teamToLeave));
	}

	public TeamSelection GetRequestedTeam()
	{
		return new TeamSelection(RakNetPINVOKE.TM_TeamMember_GetRequestedTeam(swigCPtr), cMemoryOwn: true);
	}

	public void GetRequestedSpecificTeams(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t requestedTeams)
	{
		RakNetPINVOKE.TM_TeamMember_GetRequestedSpecificTeams(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t.getCPtr(requestedTeams));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool HasRequestedTeam(TM_Team team)
	{
		return RakNetPINVOKE.TM_TeamMember_HasRequestedTeam(swigCPtr, TM_Team.getCPtr(team));
	}

	public uint GetRequestedTeamIndex(TM_Team team)
	{
		return RakNetPINVOKE.TM_TeamMember_GetRequestedTeamIndex(swigCPtr, TM_Team.getCPtr(team));
	}

	public uint GetRequestedTeamCount()
	{
		return RakNetPINVOKE.TM_TeamMember_GetRequestedTeamCount(swigCPtr);
	}

	public bool CancelTeamRequest(TM_Team specificTeamToCancel)
	{
		return RakNetPINVOKE.TM_TeamMember_CancelTeamRequest(swigCPtr, TM_Team.getCPtr(specificTeamToCancel));
	}

	public bool LeaveTeam(TM_Team team, byte _noTeamSubcategory)
	{
		return RakNetPINVOKE.TM_TeamMember_LeaveTeam(swigCPtr, TM_Team.getCPtr(team), _noTeamSubcategory);
	}

	public bool LeaveAllTeams(byte noTeamSubcategory)
	{
		return RakNetPINVOKE.TM_TeamMember_LeaveAllTeams(swigCPtr, noTeamSubcategory);
	}

	public TM_Team GetCurrentTeam()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_TeamMember_GetCurrentTeam(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new TM_Team(intPtr, cMemoryOwn: false) : null;
	}

	public uint GetCurrentTeamCount()
	{
		return RakNetPINVOKE.TM_TeamMember_GetCurrentTeamCount(swigCPtr);
	}

	public TM_Team GetCurrentTeamByIndex(uint index)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_TeamMember_GetCurrentTeamByIndex(swigCPtr, index);
		return (!(intPtr == IntPtr.Zero)) ? new TM_Team(intPtr, cMemoryOwn: false) : null;
	}

	public void GetCurrentTeams(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t _teams)
	{
		RakNetPINVOKE.TM_TeamMember_GetCurrentTeams(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t.getCPtr(_teams));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void GetLastTeams(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t _teams)
	{
		RakNetPINVOKE.TM_TeamMember_GetLastTeams(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t.getCPtr(_teams));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool IsOnTeam(TM_Team team)
	{
		return RakNetPINVOKE.TM_TeamMember_IsOnTeam(swigCPtr, TM_Team.getCPtr(team));
	}

	public ulong GetNetworkID()
	{
		return RakNetPINVOKE.TM_TeamMember_GetNetworkID(swigCPtr);
	}

	public TM_World GetTM_World()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_TeamMember_GetTM_World(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new TM_World(intPtr, cMemoryOwn: false) : null;
	}

	public void SerializeConstruction(BitStream constructionBitstream)
	{
		RakNetPINVOKE.TM_TeamMember_SerializeConstruction(swigCPtr, BitStream.getCPtr(constructionBitstream));
	}

	public bool DeserializeConstruction(TeamManager teamManager, BitStream constructionBitstream)
	{
		return RakNetPINVOKE.TM_TeamMember_DeserializeConstruction(swigCPtr, TeamManager.getCPtr(teamManager), BitStream.getCPtr(constructionBitstream));
	}

	public void SetOwner(SWIGTYPE_p_void o)
	{
		RakNetPINVOKE.TM_TeamMember_SetOwner(swigCPtr, SWIGTYPE_p_void.getCPtr(o));
	}

	public SWIGTYPE_p_void GetOwner()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_TeamMember_GetOwner(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_void(intPtr, futureUse: false) : null;
	}

	public byte GetNoTeamId()
	{
		return RakNetPINVOKE.TM_TeamMember_GetNoTeamId(swigCPtr);
	}

	public uint GetWorldIndex()
	{
		return RakNetPINVOKE.TM_TeamMember_GetWorldIndex(swigCPtr);
	}

	public static uint ToUint32(ulong g)
	{
		return RakNetPINVOKE.TM_TeamMember_ToUint32(g);
	}
}
