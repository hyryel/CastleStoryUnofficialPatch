using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class DownloadCompleteStruct : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ushort setID
	{
		get
		{
			return RakNetPINVOKE.DownloadCompleteStruct_setID_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.DownloadCompleteStruct_setID_set(swigCPtr, value);
		}
	}

	public uint numberOfFilesInThisSet
	{
		get
		{
			return RakNetPINVOKE.DownloadCompleteStruct_numberOfFilesInThisSet_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.DownloadCompleteStruct_numberOfFilesInThisSet_set(swigCPtr, value);
		}
	}

	public uint byteLengthOfThisSet
	{
		get
		{
			return RakNetPINVOKE.DownloadCompleteStruct_byteLengthOfThisSet_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.DownloadCompleteStruct_byteLengthOfThisSet_set(swigCPtr, value);
		}
	}

	public SystemAddress senderSystemAddress
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.DownloadCompleteStruct_senderSystemAddress_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new SystemAddress(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.DownloadCompleteStruct_senderSystemAddress_set(swigCPtr, SystemAddress.getCPtr(value));
		}
	}

	public RakNetGUID senderGuid
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.DownloadCompleteStruct_senderGuid_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakNetGUID(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.DownloadCompleteStruct_senderGuid_set(swigCPtr, RakNetGUID.getCPtr(value));
		}
	}

	internal DownloadCompleteStruct(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public DownloadCompleteStruct()
		: this(RakNetPINVOKE.new_DownloadCompleteStruct(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(DownloadCompleteStruct obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~DownloadCompleteStruct()
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
					RakNetPINVOKE.delete_DownloadCompleteStruct(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
