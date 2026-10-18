using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FLP_Printf : FileListProgress
{
	private HandleRef swigCPtr;

	internal FLP_Printf(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.FLP_Printf_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FLP_Printf()
		: this(RakNetPINVOKE.new_FLP_Printf(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FLP_Printf obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FLP_Printf()
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
					RakNetPINVOKE.delete_FLP_Printf(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public new static FLP_Printf GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.FLP_Printf_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new FLP_Printf(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(FLP_Printf i)
	{
		RakNetPINVOKE.FLP_Printf_DestroyInstance(getCPtr(i));
	}

	public override void OnAddFilesFromDirectoryStarted(FileList fileList, string dir)
	{
		RakNetPINVOKE.FLP_Printf_OnAddFilesFromDirectoryStarted(swigCPtr, FileList.getCPtr(fileList), dir);
	}

	public override void OnDirectory(FileList fileList, string dir, uint directoriesRemaining)
	{
		RakNetPINVOKE.FLP_Printf_OnDirectory(swigCPtr, FileList.getCPtr(fileList), dir, directoriesRemaining);
	}

	public override void OnFilePushesComplete(SystemAddress systemAddress, ushort setID)
	{
		RakNetPINVOKE.FLP_Printf_OnFilePushesComplete(swigCPtr, SystemAddress.getCPtr(systemAddress), setID);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void OnSendAborted(SystemAddress systemAddress)
	{
		RakNetPINVOKE.FLP_Printf_OnSendAborted(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
