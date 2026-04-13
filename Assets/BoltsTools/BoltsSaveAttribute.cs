using UnityEngine;

namespace BoltsTools
{
    public enum SavedVariableType {Any, Float, Int, Vector3, Vector2, String, Bool, Class}

    public class BoltsSaveAttribute : PropertyAttribute
    {
        public SavedVariableType filterType;
        public int saveIndex;

        public BoltsSaveAttribute(SavedVariableType filterType = SavedVariableType.Any, int saveIndex = 0)
        {
            this.filterType = filterType;
            this.saveIndex = saveIndex;
        }
    }
}