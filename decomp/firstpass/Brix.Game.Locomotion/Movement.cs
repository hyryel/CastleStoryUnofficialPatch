using System;

namespace Brix.Game.Locomotion;

[Serializable]
public class Movement : LocomotionStateUpdater
{
	public InPlace InPlace = new InPlace();

	public Walking Walking = new Walking();

	public Running Running = new Running();

	public Halting Halting = new Halting();

	public ClimbUp ClimbUp = new ClimbUp();

	public StepDown StepDown = new StepDown();

	public Ladder Ladder = new Ladder();

	public JumpingOff JumpingOff = new JumpingOff();

	public TurnOnSpot TurnOnSpot = new TurnOnSpot();

	public CloseStep CloseStep = new CloseStep();

	public CloseStepStart CloseStepStart = new CloseStepStart();

	public CloseStepEnd CloseStepEnd = new CloseStepEnd();

	public CutWood CutWood = new CutWood();

	public HitBrick HitBrick = new HitBrick();

	public UsePick UsePick = new UsePick();
}
