using System;
using NaughtyAttributes;
using UnityEngine;

public class EventTest : MonoBehaviour
{
    public BoltsEvent test = new BoltsEvent();

    public void tesfdt()
    {
        print("YEAH!");
    }

    public void uisg(float testjf, bool jfdk)
    {
        print(testjf + "    " + jfdk);
    }

    [Button]
    void TestEvent()
    {
        test.Invoke();
    }

    [Button]
    void AddRuntimeListener()
    {
        test.AddListener(() => uisg(50, false));
    }
}