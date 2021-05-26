public abstract class AIFocusCharacterStateTemplate : AIStateTemplate
{
	[System.NonSerialized] protected Character engaging;
	public AIFocusCharacterStateTemplate EngageWith(Character c)
	{
		engaging = c;
		return this;
	}
}


