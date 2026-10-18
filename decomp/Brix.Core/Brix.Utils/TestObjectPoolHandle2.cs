using UnityEngine;

namespace Brix.Utils;

public class TestObjectPoolHandle2
{
	public class A : IResetable
	{
		public int X = 2;

		public void ResetObject()
		{
			X = 0;
		}
	}

	public static void Test()
	{
		ObjectPoolHandle2<A> objectPoolHandle = ObjectPoolHandle2<A>.Request();
		Print(objectPoolHandle);
		objectPoolHandle.Value.X = 3;
		Print(objectPoolHandle);
		ObjectPoolHandle2<A> objectPoolHandle2 = ObjectPoolHandle2<A>.Request();
		Print(objectPoolHandle2);
		objectPoolHandle2.Dispose();
		Print(objectPoolHandle2);
		objectPoolHandle.Dispose();
		Print(objectPoolHandle);
		using ObjectPoolHandle2<A> objectPoolHandle3 = ObjectPoolHandle2<A>.Request();
		Print(objectPoolHandle3);
		objectPoolHandle3.Value.X = 5;
		Print(objectPoolHandle3);
	}

	public static void Print(ObjectPoolHandle2<A> h)
	{
		if (h.IsAlive)
		{
			Print(h.Value);
		}
		else
		{
			Debug.Log("Handle is dead!");
		}
	}

	public static void Print(A a)
	{
		Debug.Log("X=" + a.X);
	}
}
