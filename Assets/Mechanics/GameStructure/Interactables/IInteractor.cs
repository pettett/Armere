using UnityEngine;

public interface IInteractor
{
    Transform transform { get; }

    void PauseControl();
    void ResumeControl();

}