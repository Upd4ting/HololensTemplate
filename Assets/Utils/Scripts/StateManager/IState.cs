using UnityEngine;

public abstract class IState
{
    public abstract bool IsFinished();
    public abstract object[] GetParams();
    public void OnStart(params object[] args)
    {}

    public void OnUpdate()
    {}

    public void OnFixedUpdate()
    {}

    public void OnCancel()
    {}

    public void OnStop()
    {}
}