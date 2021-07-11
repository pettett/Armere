public abstract class AnimalStateTemplate : StateTemplate<AnimalState, AnimalMachine, AnimalStateTemplate>
{

}
public abstract class AnimalStateContextTemplate<T> : AnimalStateTemplate
{
	public abstract AnimalState StartContext(AnimalMachine machine, T context);
}