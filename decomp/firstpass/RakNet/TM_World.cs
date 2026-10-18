using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class TM_World : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal TM_World(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public TM_World()
		: this(RakNetPINVOKE.new_TM_World(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(TM_World obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~TM_World()
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
					RakNetPINVOKE.delete_TM_World(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public TeamManager GetTeamManager()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_World_GetTeamManager(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new TeamManager(intPtr, cMemoryOwn: false) : null;
	}

	public void AddParticipant(RakNetGUID rakNetGUID)
	{
		RakNetPINVOKE.TM_World_AddParticipant(swigCPtr, RakNetGUID.getCPtr(rakNetGUID));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveParticipant(RakNetGUID rakNetGUID)
	{
		RakNetPINVOKE.TM_World_RemoveParticipant(swigCPtr, RakNetGUID.getCPtr(rakNetGUID));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetAutoManageConnections(bool autoAdd)
	{
		RakNetPINVOKE.TM_World_SetAutoManageConnections(swigCPtr, autoAdd);
	}

	public void GetParticipantList(RakNetListRakNetGUID participantList)
	{
		RakNetPINVOKE.TM_World_GetParticipantList(swigCPtr, RakNetListRakNetGUID.getCPtr(participantList));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void ReferenceTeam(TM_Team team, ulong networkId, bool applyBalancing)
	{
		RakNetPINVOKE.TM_World_ReferenceTeam(swigCPtr, TM_Team.getCPtr(team), networkId, applyBalancing);
	}

	public void DereferenceTeam(TM_Team team, byte noTeamSubcategory)
	{
		RakNetPINVOKE.TM_World_DereferenceTeam(swigCPtr, TM_Team.getCPtr(team), noTeamSubcategory);
	}

	public uint GetTeamCount()
	{
		return RakNetPINVOKE.TM_World_GetTeamCount(swigCPtr);
	}

	public TM_Team GetTeamByIndex(uint index)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_World_GetTeamByIndex(swigCPtr, index);
		return (!(intPtr == IntPtr.Zero)) ? new TM_Team(intPtr, cMemoryOwn: false) : null;
	}

	public TM_Team GetTeamByNetworkID(ulong teamId)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_World_GetTeamByNetworkID(swigCPtr, teamId);
		return (!(intPtr == IntPtr.Zero)) ? new TM_Team(intPtr, cMemoryOwn: false) : null;
	}

	public uint GetTeamIndex(TM_Team team)
	{
		return RakNetPINVOKE.TM_World_GetTeamIndex(swigCPtr, TM_Team.getCPtr(team));
	}

	public void ReferenceTeamMember(TM_TeamMember teamMember, ulong networkId)
	{
		RakNetPINVOKE.TM_World_ReferenceTeamMember(swigCPtr, TM_TeamMember.getCPtr(teamMember), networkId);
	}

	public void DereferenceTeamMember(TM_TeamMember teamMember)
	{
		RakNetPINVOKE.TM_World_DereferenceTeamMember(swigCPtr, TM_TeamMember.getCPtr(teamMember));
	}

	public uint GetTeamMemberCount()
	{
		return RakNetPINVOKE.TM_World_GetTeamMemberCount(swigCPtr);
	}

	public TM_TeamMember GetTeamMemberByIndex(uint index)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_World_GetTeamMemberByIndex(swigCPtr, index);
		return (!(intPtr == IntPtr.Zero)) ? new TM_TeamMember(intPtr, cMemoryOwn: false) : null;
	}

	public ulong GetTeamMemberIDByIndex(uint index)
	{
		return RakNetPINVOKE.TM_World_GetTeamMemberIDByIndex(swigCPtr, index);
	}

	public TM_TeamMember GetTeamMemberByNetworkID(ulong teamMemberId)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_World_GetTeamMemberByNetworkID(swigCPtr, teamMemberId);
		return (!(intPtr == IntPtr.Zero)) ? new TM_TeamMember(intPtr, cMemoryOwn: false) : null;
	}

	public uint GetTeamMemberIndex(TM_TeamMember teamMember)
	{
		return RakNetPINVOKE.TM_World_GetTeamMemberIndex(swigCPtr, TM_TeamMember.getCPtr(teamMember));
	}

	public bool SetBalanceTeams(bool balanceTeams, byte noTeamSubcategory)
	{
		return RakNetPINVOKE.TM_World_SetBalanceTeams(swigCPtr, balanceTeams, noTeamSubcategory);
	}

	public bool GetBalanceTeams()
	{
		return RakNetPINVOKE.TM_World_GetBalanceTeams(swigCPtr);
	}

	public void SetHost(RakNetGUID _hostGuid)
	{
		RakNetPINVOKE.TM_World_SetHost(swigCPtr, RakNetGUID.getCPtr(_hostGuid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakNetGUID GetHost()
	{
		return new RakNetGUID(RakNetPINVOKE.TM_World_GetHost(swigCPtr), cMemoryOwn: true);
	}

	public byte GetWorldId()
	{
		return RakNetPINVOKE.TM_World_GetWorldId(swigCPtr);
	}

	public void Clear()
	{
		RakNetPINVOKE.TM_World_Clear(swigCPtr);
	}

	public static int JoinRequestHelperComp(SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper key, SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper data)
	{
		int result = RakNetPINVOKE.TM_World_JoinRequestHelperComp(SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper.getCPtr(key), SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper.getCPtr(data));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
