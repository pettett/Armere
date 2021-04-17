
public abstract class State<CharacterT>
	where CharacterT : Character
{
	public readonly CharacterT c;



	public State(CharacterT c)
	{
		this.c = c;
	}

	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void LateUpdate() { }
	public virtual void FixedUpdate() { }
	public virtual void End() { }
	public virtual void OnDrawGizmos() { }
}
