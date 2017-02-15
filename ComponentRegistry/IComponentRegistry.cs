using UnityEngine;
using System.Collections;

public abstract class IComponentRegistry : MonoBehaviour
{
    protected virtual void Start()
    {
        this.RegisterComponent();
    }

    protected virtual void OnDestroy()
    {
        this.UnregisterComponent();
    }
}
