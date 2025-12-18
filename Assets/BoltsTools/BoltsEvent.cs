using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class BoltsEventParameter
{
    public enum ParameterType
    {
        Int,
        Float,
        String,
        Bool,
        Object
    }

    public ParameterType type;
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public UnityEngine.Object objectValue;

    public object GetValue()
    {
        switch (type)
        {
            case ParameterType.Int:
                return intValue;
            case ParameterType.Float:
                return floatValue;
            case ParameterType.String:
                return stringValue;
            case ParameterType.Bool:
                return boolValue;
            case ParameterType.Object:
                return objectValue;
            default:
                return null;
        }
    }
}

[Serializable]
public class BoltsEventListener
{
    public GameObject targetGameObject;
    public UnityEngine.Object targetComponent;
    public string methodName;

    public List<BoltsEventParameter> parameters = new List<BoltsEventParameter>();

    public UnityEngine.Object GetTarget() { return targetComponent != null ? targetComponent : targetGameObject; }
}

[Serializable]
public class BoltsEvent
{
    [SerializeField]
    List<BoltsEventListener> persistentListeners = new List<BoltsEventListener>();

    List<Action> runtimeListeners0 = new List<Action>();
    List<Delegate> runtimeListenersWithParams = new List<Delegate>();

    public void AddListener(Action listener)
    {
        if (listener == null)
        {
            Debug.Log("Attempted to add null listener to CustomEvent");
            return;
        }

        runtimeListeners0.Add(listener);
    }

    public void AddListener(Delegate listener)
    {
        if (listener == null)
        {
            Debug.Log("Attempted to add null listener to CustomEvent");
            return;
        }

        runtimeListenersWithParams.Add(listener);
    }

    public void RemoveListener(Action listener)
    {
        if (listener != null)
            runtimeListeners0.Remove(listener);
    }

    public void RemoveListener(Delegate listener)
    {
        if (listener != null)
            runtimeListenersWithParams.Remove(listener);
    }

    public void RemoveAllListeners()
    {
        runtimeListeners0.Clear();
        runtimeListenersWithParams.Clear();
    }

    public void Invoke()
    {
        InvokePersistentListeners();

        var listenersCopy = new List<Action>(runtimeListeners0);
        foreach (var listener in listenersCopy)
        {
            try
            {
                listener?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log($"Error Invoking Event: {e.Message} \n{e.StackTrace}");
            }
        }
    }

    public void Invoke(params object[] args)
    {
        InvokePersistentListeners(args);

        var listenerCopy = new List<Delegate>(runtimeListenersWithParams);

        foreach (var listener in listenerCopy)
        {
            try { listener?.DynamicInvoke(args); }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking CustomEvent listener: {e.Message}\n{e.StackTrace}");
            }
        }

        Invoke();
    }

    void InvokePersistentListeners(params object[] runtimeArgs)
    {
        foreach (var listener in persistentListeners)
        {
            var target = listener.GetTarget();
            if (target != null && !string.IsNullOrEmpty(listener.methodName))
            {
                try
                {
                    var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    MethodInfo targetMethod = null;

                    foreach (var method in methods)
                    {
                        if (method.Name == listener.methodName)
                        {
                            targetMethod = method;
                            break;
                        }
                    }

                    if (targetMethod != null)
                    {
                        var methodParam = targetMethod.GetParameters();

                        object[] invokeArgs;

                        if (runtimeArgs != null && runtimeArgs.Length > 0)
                        {
                            invokeArgs = runtimeArgs;
                        }
                        else
                        {
                            invokeArgs = new object[methodParam.Length];

                            for (int i = 0; i < methodParam.Length; i++)
                            {
                                invokeArgs[i] = listener.parameters[i].GetValue();
                            }
                        }

                        targetMethod.Invoke(listener.targetComponent, invokeArgs);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking persistent listener {listener.methodName} on {listener.targetComponent.name}: {e.Message}");
                }
            }
        }
    }

    public int GetListenerCount()
    {
        return persistentListeners.Count + runtimeListeners0.Count + runtimeListenersWithParams.Count;
    }

    public int GetPersistentListenerCount()
    {
        return persistentListeners.Count;
    }

    public int GetRuntimeListenerCount()
    {
        return runtimeListeners0.Count + runtimeListenersWithParams.Count;
    }
}