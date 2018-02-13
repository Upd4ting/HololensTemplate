using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Draggable : MonoBehaviour
{
    public abstract void OnHoloDragEnter();
    public abstract void OnHoloDragComplete();
    public abstract void OnHoloDragCancel();
}
