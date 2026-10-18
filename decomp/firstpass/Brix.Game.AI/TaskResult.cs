namespace Brix.Game.AI;

public enum TaskResult
{
	Succes,
	WorkerFailed,
	WorkerLeftProject,
	ProjectCancelledTask,
	GoalWasRemoved,
	Switched,
	PathFailed,
	BlockedByTransferable,
	InvalidSearch,
	ReceivedNewInstruction,
	CouldNotReach
}
