using UnityEngine;

namespace BoltsTools
{
    public class BoltsAnimationParamAttribute : PropertyAttribute
    {
        public string animator;
        public AnimatorControllerParameterType? filterType;

        public BoltsAnimationParamAttribute(string animator)
        {
            this.animator = animator;
            filterType = null;
        }

        public BoltsAnimationParamAttribute(string animator, AnimatorControllerParameterType filterType)
        {
            this.animator = animator;
            this.filterType = filterType;
        }
    }
}
