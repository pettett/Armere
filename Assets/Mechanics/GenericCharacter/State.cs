[System.Serializable]
public abstract class State
{
    public virtual void Start() { }
    public virtual void Start(params object[] parameters) { Start(); }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void FixedUpdate() { }
    public virtual void End() { }
    public virtual void OnDrawGizmos() { }
}