using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileListTransferCBInterface : IDisposable
{
	public delegate bool SwigDelegateFileListTransferCBInterface_0(IntPtr onFileStruct);

	public delegate void SwigDelegateFileListTransferCBInterface_1(IntPtr fps);

	public delegate bool SwigDelegateFileListTransferCBInterface_2();

	public delegate bool SwigDelegateFileListTransferCBInterface_3(IntPtr dcs);

	public delegate void SwigDelegateFileListTransferCBInterface_4();

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private SwigDelegateFileListTransferCBInterface_0 swigDelegate0;

	private SwigDelegateFileListTransferCBInterface_1 swigDelegate1;

	private SwigDelegateFileListTransferCBInterface_2 swigDelegate2;

	private SwigDelegateFileListTransferCBInterface_3 swigDelegate3;

	private SwigDelegateFileListTransferCBInterface_4 swigDelegate4;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(OnFileStruct) };

	private static Type[] swigMethodTypes1 = new Type[1] { typeof(FileProgressStruct) };

	private static Type[] swigMethodTypes2 = new Type[0];

	private static Type[] swigMethodTypes3 = new Type[1] { typeof(DownloadCompleteStruct) };

	private static Type[] swigMethodTypes4 = new Type[0];

	internal FileListTransferCBInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileListTransferCBInterface()
		: this(RakNetPINVOKE.new_FileListTransferCBInterface(), cMemoryOwn: true)
	{
		SwigDirectorConnect();
	}

	internal static HandleRef getCPtr(FileListTransferCBInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileListTransferCBInterface()
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
					RakNetPINVOKE.delete_FileListTransferCBInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual bool OnFile(OnFileStruct onFileStruct)
	{
		return RakNetPINVOKE.FileListTransferCBInterface_OnFile(swigCPtr, OnFileStruct.getCPtr(onFileStruct));
	}

	public virtual void OnFileProgress(FileProgressStruct fps)
	{
		RakNetPINVOKE.FileListTransferCBInterface_OnFileProgress(swigCPtr, FileProgressStruct.getCPtr(fps));
	}

	public virtual bool Update()
	{
		return (!SwigDerivedClassHasMethod("Update", swigMethodTypes2)) ? RakNetPINVOKE.FileListTransferCBInterface_Update(swigCPtr) : RakNetPINVOKE.FileListTransferCBInterface_UpdateSwigExplicitFileListTransferCBInterface(swigCPtr);
	}

	public virtual bool OnDownloadComplete(DownloadCompleteStruct dcs)
	{
		return (!SwigDerivedClassHasMethod("OnDownloadComplete", swigMethodTypes3)) ? RakNetPINVOKE.FileListTransferCBInterface_OnDownloadComplete(swigCPtr, DownloadCompleteStruct.getCPtr(dcs)) : RakNetPINVOKE.FileListTransferCBInterface_OnDownloadCompleteSwigExplicitFileListTransferCBInterface(swigCPtr, DownloadCompleteStruct.getCPtr(dcs));
	}

	public virtual void OnDereference()
	{
		if (SwigDerivedClassHasMethod("OnDereference", swigMethodTypes4))
		{
			RakNetPINVOKE.FileListTransferCBInterface_OnDereferenceSwigExplicitFileListTransferCBInterface(swigCPtr);
		}
		else
		{
			RakNetPINVOKE.FileListTransferCBInterface_OnDereference(swigCPtr);
		}
	}

	private void SwigDirectorConnect()
	{
		if (SwigDerivedClassHasMethod("OnFile", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorOnFile;
		}
		if (SwigDerivedClassHasMethod("OnFileProgress", swigMethodTypes1))
		{
			swigDelegate1 = SwigDirectorOnFileProgress;
		}
		if (SwigDerivedClassHasMethod("Update", swigMethodTypes2))
		{
			swigDelegate2 = SwigDirectorUpdate;
		}
		if (SwigDerivedClassHasMethod("OnDownloadComplete", swigMethodTypes3))
		{
			swigDelegate3 = SwigDirectorOnDownloadComplete;
		}
		if (SwigDerivedClassHasMethod("OnDereference", swigMethodTypes4))
		{
			swigDelegate4 = SwigDirectorOnDereference;
		}
		RakNetPINVOKE.FileListTransferCBInterface_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3, swigDelegate4);
	}

	private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
	{
		MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, methodTypes, null);
		return method.DeclaringType.IsSubclassOf(typeof(FileListTransferCBInterface));
	}

	private bool SwigDirectorOnFile(IntPtr onFileStruct)
	{
		return OnFile((!(onFileStruct == IntPtr.Zero)) ? new OnFileStruct(onFileStruct, cMemoryOwn: false) : null);
	}

	private void SwigDirectorOnFileProgress(IntPtr fps)
	{
		OnFileProgress((!(fps == IntPtr.Zero)) ? new FileProgressStruct(fps, cMemoryOwn: false) : null);
	}

	private bool SwigDirectorUpdate()
	{
		return Update();
	}

	private bool SwigDirectorOnDownloadComplete(IntPtr dcs)
	{
		return OnDownloadComplete((!(dcs == IntPtr.Zero)) ? new DownloadCompleteStruct(dcs, cMemoryOwn: false) : null);
	}

	private void SwigDirectorOnDereference()
	{
		OnDereference();
	}
}
