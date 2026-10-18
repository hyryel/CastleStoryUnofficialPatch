using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class TM_Team : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal TM_Team(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public TM_Team()
		: this(RakNetPINVOKE.new_TM_Team(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(TM_Team obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~TM_Team()
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
					RakNetPINVOKE.delete_TM_Team(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public static TM_Team GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_Team_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new TM_Team(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(TM_Team i)
	{
		RakNetPINVOKE.TM_Team_DestroyInstance(getCPtr(i));
	}

	public bool SetMemberLimit(ushort _teamMemberLimit, byte noTeamSubcategory)
	{
		return RakNetPINVOKE.TM_Team_SetMemberLimit(swigCPtr, _teamMemberLimit, noTeamSubcategory);
	}

	public ushort GetMemberLimit()
	{
		return RakNetPINVOKE.TM_Team_GetMemberLimit(swigCPtr);
	}

	public ushort GetMemberLimitSetting()
	{
		return RakNetPINVOKE.TM_Team_GetMemberLimitSetting(swigCPtr);
	}

	public bool SetJoinPermissions(byte _joinPermissions)
	{
		return RakNetPINVOKE.TM_Team_SetJoinPermissions(swigCPtr, _joinPermissions);
	}

	public byte GetJoinPermissions()
	{
		return RakNetPINVOKE.TM_Team_GetJoinPermissions(swigCPtr);
	}

	public void LeaveTeam(TM_TeamMember teamMember, byte noTeamSubcategory)
	{
		RakNetPINVOKE.TM_Team_LeaveTeam(swigCPtr, TM_TeamMember.getCPtr(teamMember), noTeamSubcategory);
	}

	public bool GetBalancingApplies()
	{
		return RakNetPINVOKE.TM_Team_GetBalancingApplies(swigCPtr);
	}

	public void GetTeamMembers(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_TeamMember_p_t _teamMembers)
	{
		RakNetPINVOKE.TM_Team_GetTeamMembers(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNet__TM_TeamMember_p_t.getCPtr(_teamMembers));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public uint GetTeamMembersCount()
	{
		return RakNetPINVOKE.TM_Team_GetTeamMembersCount(swigCPtr);
	}

	public TM_TeamMember GetTeamMemberByIndex(uint index)
	{
		IntPtr intPtr = RakNetPINVOKE.TM_Team_GetTeamMemberByIndex(swigCPtr, index);
		return (!(intPtr == IntPtr.Zero)) ? new TM_TeamMember(intPtr, cMemoryOwn: false) : null;
	}

	public ulong GetNetworkID()
	{
		return RakNetPINVOKE.TM_Team_GetNetworkID(swigCPtr);
	}

	public TM_World GetTM_World()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_Team_GetTM_World(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new TM_World(intPtr, cMemoryOwn: false) : null;
	}

	public void SerializeConstruction(BitStream constructionBitstream)
	{
		RakNetPINVOKE.TM_Team_SerializeConstruction(swigCPtr, BitStream.getCPtr(constructionBitstream));
	}

	public bool DeserializeConstruction(TeamManager teamManager, BitStream constructionBitstream)
	{
		return RakNetPINVOKE.TM_Team_DeserializeConstruction(swigCPtr, TeamManager.getCPtr(teamManager), BitStream.getCPtr(constructionBitstream));
	}

	public void SetOwner(SWIGTYPE_p_void o)
	{
		RakNetPINVOKE.TM_Team_SetOwner(swigCPtr, SWIGTYPE_p_void.getCPtr(o));
	}

	public SWIGTYPE_p_void GetOwner()
	{
		IntPtr intPtr = RakNetPINVOKE.TM_Team_GetOwner(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_void(intPtr, futureUse: false) : null;
	}

	public uint GetWorldIndex()
	{
		return RakNetPINVOKE.TM_Team_GetWorldIndex(swigCPtr);
	}

	public static uint ToUint32(ulong g)
	{
		return RakNetPINVOKE.TM_Team_ToUint32(g);
	}
}
