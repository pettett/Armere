


using UnityEngine;

public class PlayerRelativeObject : MonoBehaviour
{
    public float enableRange = 50;
    public float disableRange = 60;
    protected void Start()
    {
        PlayerController.Player_CharacterController.relativeObjects.Add(this);
    }
    protected void OnDestroy()
    {
        PlayerController.Player_CharacterController.relativeObjects.Remove(this);
    }
    public virtual void Enable() { enabled = true; }
    public virtual void Disable() { enabled = false; }
}

