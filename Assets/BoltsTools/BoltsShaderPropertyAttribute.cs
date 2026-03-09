using UnityEngine;

namespace BoltsTools
{
    public class BoltsShaderPropertyAttribute : PropertyAttribute
    {
        public string materialField;

        public BoltsShaderPropertyAttribute(string materialField)
        {
            this.materialField = materialField;
        }
    }
}