
public abstract class State<StateT, MachineT, TemplateT>
	where MachineT : StateMachine<StateT, MachineT, TemplateT>
	where StateT : State<StateT, MachineT, TemplateT>
	where TemplateT : StateTemplate<StateT, MachineT, TemplateT>
{
	public readonly MachineT machine;

	public State(MachineT machine)
	{
		this.machine = machine;
	}

	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void LateUpdate() { }
	public virtual void FixedUpdate() { }
	public virtual void End() { }
	public virtual void OnDrawGizmos() { }
}
