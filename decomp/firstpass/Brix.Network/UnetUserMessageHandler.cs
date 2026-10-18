using Brix.Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class UnetUserMessageHandler : UNetAbstractMessageHandler
{
	public class AssignUserObjectMessage : MessageBase
	{
		public GameObject obj;

		public AssignUserObjectMessage()
		{
		}

		public AssignUserObjectMessage(GameObject Obj)
		{
			obj = Obj;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(obj);
		}

		public override void Deserialize(NetworkReader reader)
		{
			obj = reader.ReadGameObject();
		}
	}

	public override void Initialize()
	{
		RegisterHandlers();
	}

	protected override void RegisterHandlers()
	{
		RegisterHandler(54, AssignUserObject);
	}

	private void AssignUserObject(NetworkMessage message)
	{
		AssignUserObjectMessage assignUserObjectMessage = message.ReadMessage<AssignUserObjectMessage>();
		User.LocalUser = assignUserObjectMessage.obj.GetComponent<User>();
	}
}
