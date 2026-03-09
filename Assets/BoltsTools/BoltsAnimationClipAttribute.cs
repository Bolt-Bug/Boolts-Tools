using UnityEngine;

namespace BoltsTools
{
    public class BoltsAnimationClipAttribute : PropertyAttribute
    {
        public string animator;

        public BoltsAnimationClipAttribute(string animator)
        {
            this.animator = animator;
        }
    }
}
