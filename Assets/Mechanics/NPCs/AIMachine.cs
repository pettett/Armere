public class AIMachine : StateMachine<AIState, AIMachine, AIStateTemplate>
{
	public AIHumanoid c;


	public void ChangeToState<T>(AIContextStateTemplate<T> state, T context) => ChangeToState(state.Target(context));
}