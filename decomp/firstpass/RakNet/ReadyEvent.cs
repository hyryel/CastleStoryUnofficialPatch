using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class ReadyEvent : PluginInterface2
{
	private HandleRef swigCPtr;

	internal ReadyEvent(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.ReadyEvent_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public ReadyEvent()
		: this(RakNetPINVOKE.new_ReadyEvent(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(ReadyEvent obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~ReadyEvent()
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
					RakNetPINVOKE.delete_ReadyEvent(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static ReadyEvent GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.ReadyEvent_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new ReadyEvent(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(ReadyEvent i)
	{
		RakNetPINVOKE.ReadyEvent_DestroyInstance(getCPtr(i));
	}

	public bool SetEvent(int eventId, bool isReady)
	{
		return RakNetPINVOKE.ReadyEvent_SetEvent(swigCPtr, eventId, isReady);
	}

	public void ForceCompletion(int eventId)
	{
		RakNetPINVOKE.ReadyEvent_ForceCompletion(swigCPtr, eventId);
	}

	public bool DeleteEvent(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_DeleteEvent(swigCPtr, eventId);
	}

	public bool IsEventSet(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_IsEventSet(swigCPtr, eventId);
	}

	public bool IsEventCompletionProcessing(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_IsEventCompletionProcessing(swigCPtr, eventId);
	}

	public bool IsEventCompleted(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_IsEventCompleted(swigCPtr, eventId);
	}

	public bool HasEvent(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_HasEvent(swigCPtr, eventId);
	}

	public uint GetEventListSize()
	{
		return RakNetPINVOKE.ReadyEvent_GetEventListSize(swigCPtr);
	}

	public int GetEventAtIndex(uint index)
	{
		return RakNetPINVOKE.ReadyEvent_GetEventAtIndex(swigCPtr, index);
	}

	public bool AddToWaitList(int eventId, RakNetGUID guid)
	{
		bool result = RakNetPINVOKE.ReadyEvent_AddToWaitList(swigCPtr, eventId, RakNetGUID.getCPtr(guid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RemoveFromWaitList(int eventId, RakNetGUID guid)
	{
		bool result = RakNetPINVOKE.ReadyEvent_RemoveFromWaitList(swigCPtr, eventId, RakNetGUID.getCPtr(guid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool IsInWaitList(int eventId, RakNetGUID guid)
	{
		bool result = RakNetPINVOKE.ReadyEvent_IsInWaitList(swigCPtr, eventId, RakNetGUID.getCPtr(guid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public uint GetRemoteWaitListSize(int eventId)
	{
		return RakNetPINVOKE.ReadyEvent_GetRemoteWaitListSize(swigCPtr, eventId);
	}

	public RakNetGUID GetFromWaitListAtIndex(int eventId, uint index)
	{
		return new RakNetGUID(RakNetPINVOKE.ReadyEvent_GetFromWaitListAtIndex(swigCPtr, eventId, index), cMemoryOwn: true);
	}

	public ReadyEventSystemStatus GetReadyStatus(int eventId, RakNetGUID guid)
	{
		ReadyEventSystemStatus result = (ReadyEventSystemStatus)RakNetPINVOKE.ReadyEvent_GetReadyStatus(swigCPtr, eventId, RakNetGUID.getCPtr(guid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSendChannel(byte newChannel)
	{
		RakNetPINVOKE.ReadyEvent_SetSendChannel(swigCPtr, newChannel);
	}

	public static int RemoteSystemCompByGuid(RakNetGUID key, SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem data)
	{
		int result = RakNetPINVOKE.ReadyEvent_RemoteSystemCompByGuid(RakNetGUID.getCPtr(key), SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem.getCPtr(data));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
