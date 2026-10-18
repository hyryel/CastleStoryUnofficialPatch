using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileListTransfer : PluginInterface2
{
	private HandleRef swigCPtr;

	internal FileListTransfer(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.FileListTransfer_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileListTransfer()
		: this(RakNetPINVOKE.new_FileListTransfer(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FileListTransfer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileListTransfer()
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
					RakNetPINVOKE.delete_FileListTransfer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static FileListTransfer GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.FileListTransfer_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new FileListTransfer(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(FileListTransfer i)
	{
		RakNetPINVOKE.FileListTransfer_DestroyInstance(getCPtr(i));
	}

	public void StartIncrementalReadThreads(int numThreads, int threadPriority)
	{
		RakNetPINVOKE.FileListTransfer_StartIncrementalReadThreads__SWIG_0(swigCPtr, numThreads, threadPriority);
	}

	public void StartIncrementalReadThreads(int numThreads)
	{
		RakNetPINVOKE.FileListTransfer_StartIncrementalReadThreads__SWIG_1(swigCPtr, numThreads);
	}

	public ushort SetupReceive(FileListTransferCBInterface handler, bool deleteHandler, SystemAddress allowedSender)
	{
		ushort result = RakNetPINVOKE.FileListTransfer_SetupReceive(swigCPtr, FileListTransferCBInterface.getCPtr(handler), deleteHandler, SystemAddress.getCPtr(allowedSender));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Send(FileList fileList, RakPeerInterface rakPeer, SystemAddress recipient, ushort setID, PacketPriority priority, char orderingChannel, IncrementalReadInterface _incrementalReadInterface, uint _chunkSize)
	{
		RakNetPINVOKE.FileListTransfer_Send__SWIG_0(swigCPtr, FileList.getCPtr(fileList), RakPeerInterface.getCPtr(rakPeer), SystemAddress.getCPtr(recipient), setID, (int)priority, orderingChannel, IncrementalReadInterface.getCPtr(_incrementalReadInterface), _chunkSize);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Send(FileList fileList, RakPeerInterface rakPeer, SystemAddress recipient, ushort setID, PacketPriority priority, char orderingChannel, IncrementalReadInterface _incrementalReadInterface)
	{
		RakNetPINVOKE.FileListTransfer_Send__SWIG_1(swigCPtr, FileList.getCPtr(fileList), RakPeerInterface.getCPtr(rakPeer), SystemAddress.getCPtr(recipient), setID, (int)priority, orderingChannel, IncrementalReadInterface.getCPtr(_incrementalReadInterface));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Send(FileList fileList, RakPeerInterface rakPeer, SystemAddress recipient, ushort setID, PacketPriority priority, char orderingChannel)
	{
		RakNetPINVOKE.FileListTransfer_Send__SWIG_2(swigCPtr, FileList.getCPtr(fileList), RakPeerInterface.getCPtr(rakPeer), SystemAddress.getCPtr(recipient), setID, (int)priority, orderingChannel);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public uint GetPendingFilesToAddress(SystemAddress recipient)
	{
		uint result = RakNetPINVOKE.FileListTransfer_GetPendingFilesToAddress(swigCPtr, SystemAddress.getCPtr(recipient));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void CancelReceive(ushort setId)
	{
		RakNetPINVOKE.FileListTransfer_CancelReceive(swigCPtr, setId);
	}

	public void RemoveReceiver(SystemAddress systemAddress)
	{
		RakNetPINVOKE.FileListTransfer_RemoveReceiver(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool IsHandlerActive(ushort setId)
	{
		return RakNetPINVOKE.FileListTransfer_IsHandlerActive(swigCPtr, setId);
	}

	public void AddCallback(FileListProgress cb)
	{
		RakNetPINVOKE.FileListTransfer_AddCallback(swigCPtr, FileListProgress.getCPtr(cb));
	}

	public void RemoveCallback(FileListProgress cb)
	{
		RakNetPINVOKE.FileListTransfer_RemoveCallback(swigCPtr, FileListProgress.getCPtr(cb));
	}

	public void ClearCallbacks()
	{
		RakNetPINVOKE.FileListTransfer_ClearCallbacks(swigCPtr);
	}
}
