using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    /// <summary>
    /// Publisher (InstantiatedRoom class) calls this method to invoke the OnRoomChanged event
    /// </summary>
    /// <param name="room"></param>
    public static void CallRoomChangedEvent(Room room)
    {
        // If there are no subscribers to the OnRoomChanged event (identified using the "?" operator) then do not invoke the event
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}