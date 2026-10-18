using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileListProgress : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal FileListProgress(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileListProgress()
		: this(RakNetPINVOKE.new_FileListProgress(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FileListProgress obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileListProgress()
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
					RakNetPINVOKE.delete_FileListProgress(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public static FileListProgress GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.FileListProgress_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new FileListProgress(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(FileListProgress i)
	{
		RakNetPINVOKE.FileListProgress_DestroyInstance(getCPtr(i));
	}

	public virtual void OnAddFilesFromDirectoryStarted(FileList fileList, string dir)
	{
		RakNetPINVOKE.FileListProgress_OnAddFilesFromDirectoryStarted(swigCPtr, FileList.getCPtr(fileList), dir);
	}

	public virtual void OnDirectory(FileList fileList, string dir, uint directoriesRemaining)
	{
		RakNetPINVOKE.FileListProgress_OnDirectory(swigCPtr, FileList.getCPtr(fileList), dir, directoriesRemaining);
	}

	public virtual void OnFile(FileList fileList, string dir, string fileName, uint fileSize)
	{
		RakNetPINVOKE.FileListProgress_OnFile(swigCPtr, FileList.getCPtr(fileList), dir, fileName, fileSize);
	}

	public virtual void OnFilePush(string fileName, uint fileLengthBytes, uint offset, uint bytesBeingSent, bool done, SystemAddress targetSystem, ushort setId)
	{
		RakNetPINVOKE.FileListProgress_OnFilePush(swigCPtr, fileName, fileLengthBytes, offset, bytesBeingSent, done, SystemAddress.getCPtr(targetSystem), setId);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnFilePushesComplete(SystemAddress systemAddress, ushort setId)
	{
		RakNetPINVOKE.FileListProgress_OnFilePushesComplete(swigCPtr, SystemAddress.getCPtr(systemAddress), setId);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnSendAborted(SystemAddress systemAddress)
	{
		RakNetPINVOKE.FileListProgress_OnSendAborted(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
