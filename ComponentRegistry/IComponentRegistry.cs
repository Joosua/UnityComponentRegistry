using UnityEngine;
using System.Collections;

public abstract class IComponentRegistry : MonoBehaviour
{
    protected virtual bool RegisterBaseTypes()
    {
        return false;
    }

    protected virtual void Awake()
    {
        this.RegisterComponent(RegisterBaseTypes());
    }

    protected virtual void OnDestroy()
    {
        this.UnregisterComponent(RegisterBaseTypes());
    }
}
