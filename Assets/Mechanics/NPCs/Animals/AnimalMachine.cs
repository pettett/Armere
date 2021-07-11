public class AnimalMachine : StateMachine<AnimalState, AnimalMachine, AnimalStateTemplate>
{
	public Animal c;

	public void ChangeToState<T>(AnimalStateContextTemplate<T> state, T context) => ChangeToState(state.StartContext(this, context));
}