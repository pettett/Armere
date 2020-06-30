


using UnityEngine;

public class PlayerRelativeObject : MonoBehaviour
{
    public float enableRange = 50;
    public float disableRange = 60;
    protected void AddToRegister()
    {
        PlayerController.Player_CharacterController.activePlayerController.relativeObjects.Add(this);
    }
    protected void RemoveFromRegister()
    {
        PlayerController.Player_CharacterController.activePlayerController.relativeObjects.Remove(this);
    }
    public virtual void OnPlayerInRange() { enabled = true; }
    public virtual void OnPlayerOutRange() { enabled = false; }
}

