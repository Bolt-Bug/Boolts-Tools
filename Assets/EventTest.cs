using System;
using NaughtyAttributes;
using UnityEngine;

public class EventTest : MonoBehaviour
{
    [BoltsComment("This Is A Comment With More Space", 50)]
    public string commentShowcase = "There Is A Comment Above Me :)";

    [BoltsComment("This Is A Comment")]
    public string commentShowcase2 = "There Is A Comment Above Me :)";

    public string normalString = "This IS A Normal String";

    public BoltsEvent eventTest = new BoltsEvent();

    public bool test;

    public void TestEvent()
    {
        print("YEAH!");
    }
}