using System;
using UnityEngine;

namespace BoltsTools
{
    public class BoltsToolTipAttribute : PropertyAttribute
    {
        public string msg;

        public BoltsToolTipAttribute(string msg)
        {
            this.msg = msg;
        }
    }
}
