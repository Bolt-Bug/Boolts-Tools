using UnityEngine;

public class BoltsCommentAttribute : PropertyAttribute
{
    public string comment;
    public float space;

    public BoltsCommentAttribute(string comment, float space)
    {
        this.comment = comment;
        this.space = space;
    }
}