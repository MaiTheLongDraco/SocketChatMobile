using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    public static readonly Dictionary<string,Action> ActionsQueue = new Dictionary<string, Action>();

    public static void Enqueue(string key,Action newAction)
    {
        if(newAction==null||ActionsQueue.ContainsKey(key))return;
        lock (ActionsQueue)
        {
            ActionsQueue.Add(key,newAction);
        }
    }

    public static void Dequeue(string key)
    {
        lock (ActionsQueue)
        {
            if (ActionsQueue.ContainsKey(key))
            {
                ActionsQueue[key]?.Invoke();
            } 
        }
       
    }
}
