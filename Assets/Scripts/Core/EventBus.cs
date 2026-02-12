using System;
using System.Collections.Generic;

public class EventBus
{
    private static EventBus instance;
    public static EventBus Instance
    {
        get
        {
            if (instance == null)
                instance = new EventBus();
            return instance;
        }
    }

    private Dictionary<Type, Delegate> eventDictionary = new Dictionary<Type, Delegate>();

    /// <summary>
    /// Subscription Events
    /// </summary>
    public void Subscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.TryGetValue(eventType, out Delegate existingDelegate))
        {
            eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            eventDictionary[eventType] = listener;
        }
    }

    /// <summary>
    /// Unsubscription event
    /// </summary>
    public void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.TryGetValue(eventType, out Delegate existingDelegate))
        {
            Delegate newDelegate = Delegate.Remove(existingDelegate, listener);
            
            if (newDelegate == null)
                eventDictionary.Remove(eventType);
            else
                eventDictionary[eventType] = newDelegate;
        }
    }

    /// <summary>
    /// Publish Event
    /// </summary>
    public void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.TryGetValue(eventType, out Delegate del))
        {
            (del as Action<T>)?.Invoke(eventData);
        }
    }

    /// <summary>
    /// Clear all events
    /// </summary>
    public void Clear()
    {
        eventDictionary.Clear();
    }
}