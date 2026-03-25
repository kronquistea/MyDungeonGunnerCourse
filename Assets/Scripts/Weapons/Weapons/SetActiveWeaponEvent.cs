using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class SetActiveWeaponEvent : MonoBehaviour
{
    // Subscribers subscribe to this event
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveWeapon;

    /// <summary>
    /// Publishers call this method when they want to invoke the OnSetActiveWeapon event
    /// </summary>
    /// <param name="weapon"></param>
    public void CallSetActiveWeaponEvent(Weapon weapon)
    {
        OnSetActiveWeapon?.Invoke(this, new SetActiveWeaponEventArgs() { weapon = weapon });
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
}