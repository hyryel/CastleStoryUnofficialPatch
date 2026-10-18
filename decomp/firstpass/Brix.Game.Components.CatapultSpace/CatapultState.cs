using FSM;
using StateChart;
using UnityEngine.Networking;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultState : StateMachineComponent
{
	public class MessageArmer
	{
	}

	public class MessageDesarmer
	{
	}

	public class MessagePiloter
	{
	}

	public class MessageCesserDePiloter
	{
	}

	public class MessageApercu
	{
	}

	public class MessageCesserApercu
	{
	}

	public class MessageViser
	{
	}

	public class MessageDeclancher
	{
	}

	public class MessageSelectionner
	{
	}

	public class MessageDeselectionner
	{
	}

	public class MessageDeprogrammer
	{
	}

	protected override State CreateFSM()
	{
		State state = new State("root");
		State state2 = new State("Cadre");
		state.AddRegion(state2);
		State state3 = new State("Aligned", state2);
		State state4 = new State("Turning", state2);
		state3.TransitTo<DeltaTime>(state4, Trans_Aligned_Turning);
		state4.Entry(Entry_Turning);
		state4.Exit(Exit_Turning);
		state4.React<DeltaTime>(React_Turning_DeltaTime);
		state4.TransitTo<MessageViser>(state4, Trans_Turning_Turning);
		state4.TransitTo<DeltaTime>(state3, Trans_Turning_Aligned);
		State state5 = new State("Bras");
		state.AddRegion(state5);
		State state6 = new State("Relaxed", state5);
		State state7 = new State("Cranking", state5);
		State state8 = new State("Cranked", state5);
		State state9 = new State("Firing", state5);
		State state10 = new State("Cooldown", state5);
		state6.Entry(Entry_Relaxed);
		state6.TransitTo<DeltaTime>(state7, Trans_Relaxed_Cranking);
		state7.Entry(Entry_Cranking);
		state7.Exit(Exit_Cranking);
		state7.React<DeltaTime>(React_Cranking_DeltaTime);
		state7.TransitTo<DeltaTime>(state6, Trans_Cranking_Relaxed);
		state7.TransitTo<MessageViser>(state7, Trans_Cranking_Cranking);
		state7.TransitTo<DeltaTime>(state8, Trans_Cranking_Cranked);
		state8.Entry(Entry_Cranked);
		state8.Exit(Exit_Cranked);
		state8.TransitTo<MessageViser>(state7, Trans_Cranked_Cranking);
		state8.TransitTo<DeltaTime>(state9, Trans_Cranked_Firing);
		state9.Entry(Entry_Firing);
		state9.Exit(Exit_Firing);
		state9.TransitTo<DeltaTime>(state10, Trans_Firing_Cooldown);
		state10.TransitTo<DeltaTime>(state6, Trans_Cooldown_Relaxed);
		State state11 = new State("Global");
		state.AddRegion(state11);
		State state12 = new State("Initializing", state11);
		State state13 = new State("Healthy", state11);
		state12.TransitTo<DeltaTime>(state13, Trans_Initializing_Healthy);
		state13.TransitTo<MessageDeprogrammer>(state12, Trans_Healthy_Initializing);
		state13.React<MessageArmer>(React_Healthy_Armer);
		state13.React<MessageDesarmer>(React_Healthy_Desarmer);
		state13.React<MessagePiloter>(React_Healthy_Piloter);
		state13.React<MessageCesserDePiloter>(React_Healthy_CesserDePiloter);
		state13.React<MessageApercu>(React_Healthy_Apercu);
		state13.React<MessageCesserApercu>(React_Healthy_CesserApercu);
		state13.React<MessageViser>(React_Healthy_Viser);
		state13.React<MessageDeclancher>(React_Healthy_Declancher);
		state13.React<DeltaTime>(React_Healthy_DeltaTime);
		State state14 = new State("Selection");
		state.AddRegion(state14);
		State state15 = new State("Deselected", state14);
		State state16 = new State("Selected", state14);
		state15.TransitTo<MessageSelectionner>(state16, Trans_Deselected_Selected);
		state16.TransitTo<MessageDeselectionner>(state15, Trans_Selected_Deselected);
		state15.React<DeltaTime>(React_Deselected_DeltaTime);
		state16.React<DeltaTime>(React_Selected_DeltaTime);
		State state17 = new State("Pilotage");
		state.AddRegion(state17);
		State state18 = new State("Vacant", state17);
		State state19 = new State("Driving", state17);
		State state20 = new State("Leaving", state17);
		state18.Entry(Entry_Vacant);
		state18.TransitTo<MessagePiloter>(state19, Trans_Vacant_Driving);
		state19.Entry(Entry_Driving);
		state19.TransitTo<MessageCesserDePiloter>(state20, Trans_Driving_Leaving);
		state20.Entry(Entry_Leaving);
		state20.TransitTo<DeltaTime>(state18, Trans_Leaving_Vacant);
		state20.TransitTo<MessagePiloter>(state19, Trans_Leaving_Driving);
		return state;
	}

	protected virtual void Entry_Turning()
	{
	}

	protected virtual void Exit_Turning()
	{
	}

	protected virtual void React_Turning_DeltaTime(DeltaTime m)
	{
	}

	protected virtual bool Trans_Turning_Turning(MessageViser m)
	{
		return true;
	}

	protected virtual bool Trans_Turning_Aligned(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Aligned_Turning(DeltaTime m)
	{
		return true;
	}

	protected virtual void Entry_Relaxed()
	{
	}

	protected virtual bool Trans_Relaxed_Cranking(DeltaTime m)
	{
		return true;
	}

	protected virtual void Entry_Cranking()
	{
	}

	protected virtual void Exit_Cranking()
	{
	}

	protected virtual void React_Cranking_DeltaTime(DeltaTime m)
	{
	}

	protected virtual bool Trans_Cranking_Relaxed(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Cranking_Cranking(MessageViser m)
	{
		return true;
	}

	protected virtual bool Trans_Cranking_Cranked(DeltaTime m)
	{
		return true;
	}

	protected virtual void Entry_Cranked()
	{
	}

	protected virtual void Exit_Cranked()
	{
	}

	protected virtual bool Trans_Cranked_Cranking(MessageViser m)
	{
		return true;
	}

	protected virtual bool Trans_Cranked_Firing(DeltaTime m)
	{
		return true;
	}

	protected virtual void Entry_Firing()
	{
	}

	protected virtual void Exit_Firing()
	{
	}

	protected virtual bool Trans_Firing_Cooldown(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Cooldown_Relaxed(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Initializing_Healthy(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Healthy_Initializing(MessageDeprogrammer m)
	{
		return true;
	}

	protected virtual void React_Healthy_Armer(MessageArmer m)
	{
	}

	protected virtual void React_Healthy_Desarmer(MessageDesarmer m)
	{
	}

	protected virtual void React_Healthy_Piloter(MessagePiloter m)
	{
	}

	protected virtual void React_Healthy_CesserDePiloter(MessageCesserDePiloter m)
	{
	}

	protected virtual void React_Healthy_Apercu(MessageApercu m)
	{
	}

	protected virtual void React_Healthy_CesserApercu(MessageCesserApercu m)
	{
	}

	protected virtual void React_Healthy_Viser(MessageViser m)
	{
	}

	protected virtual void React_Healthy_Declancher(MessageDeclancher m)
	{
	}

	protected virtual void React_Healthy_DeltaTime(DeltaTime m)
	{
	}

	protected virtual bool Trans_Deselected_Selected(MessageSelectionner m)
	{
		return true;
	}

	protected virtual bool Trans_Selected_Deselected(MessageDeselectionner m)
	{
		return true;
	}

	protected virtual void React_Deselected_DeltaTime(DeltaTime m)
	{
	}

	protected virtual void React_Selected_DeltaTime(DeltaTime m)
	{
	}

	protected virtual void Entry_Vacant()
	{
	}

	protected virtual bool Trans_Vacant_Driving(MessagePiloter m)
	{
		return true;
	}

	protected virtual void Entry_Driving()
	{
	}

	protected virtual bool Trans_Driving_Leaving(MessageCesserDePiloter m)
	{
		return true;
	}

	protected virtual void Entry_Leaving()
	{
	}

	protected virtual bool Trans_Leaving_Vacant(DeltaTime m)
	{
		return true;
	}

	protected virtual bool Trans_Leaving_Driving(MessagePiloter m)
	{
		return true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
