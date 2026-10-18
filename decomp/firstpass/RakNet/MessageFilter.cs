using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class MessageFilter : PluginInterface2
{
	private HandleRef swigCPtr;

	internal MessageFilter(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.MessageFilter_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public MessageFilter()
		: this(RakNetPINVOKE.new_MessageFilter(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(MessageFilter obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~MessageFilter()
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
					RakNetPINVOKE.delete_MessageFilter(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static MessageFilter GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.MessageFilter_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new MessageFilter(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(MessageFilter i)
	{
		RakNetPINVOKE.MessageFilter_DestroyInstance(getCPtr(i));
	}

	public void SetAutoAddNewConnectionsToFilter(int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetAutoAddNewConnectionsToFilter(swigCPtr, filterSetID);
	}

	public void SetAllowMessageID(bool allow, int messageIDStart, int messageIDEnd, int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetAllowMessageID(swigCPtr, allow, messageIDStart, messageIDEnd, filterSetID);
	}

	public void SetAllowRPC4(bool allow, string uniqueID, int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetAllowRPC4(swigCPtr, allow, uniqueID, filterSetID);
	}

	public void SetActionOnDisallowedMessage(bool kickOnDisallowed, bool banOnDisallowed, uint banTimeMS, int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetActionOnDisallowedMessage(swigCPtr, kickOnDisallowed, banOnDisallowed, banTimeMS, filterSetID);
	}

	public void SetFilterMaxTime(int allowedTimeMS, bool banOnExceed, uint banTimeMS, int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetFilterMaxTime(swigCPtr, allowedTimeMS, banOnExceed, banTimeMS, filterSetID);
	}

	public int GetSystemFilterSet(AddressOrGUID addressOrGUID)
	{
		int result = RakNetPINVOKE.MessageFilter_GetSystemFilterSet(swigCPtr, AddressOrGUID.getCPtr(addressOrGUID));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSystemFilterSet(AddressOrGUID addressOrGUID, int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_SetSystemFilterSet(swigCPtr, AddressOrGUID.getCPtr(addressOrGUID), filterSetID);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public uint GetSystemCount(int filterSetID)
	{
		return RakNetPINVOKE.MessageFilter_GetSystemCount(swigCPtr, filterSetID);
	}

	public uint GetFilterSetCount()
	{
		return RakNetPINVOKE.MessageFilter_GetFilterSetCount(swigCPtr);
	}

	public int GetFilterSetIDByIndex(uint index)
	{
		return RakNetPINVOKE.MessageFilter_GetFilterSetIDByIndex(swigCPtr, index);
	}

	public void DeleteFilterSet(int filterSetID)
	{
		RakNetPINVOKE.MessageFilter_DeleteFilterSet(swigCPtr, filterSetID);
	}
}
