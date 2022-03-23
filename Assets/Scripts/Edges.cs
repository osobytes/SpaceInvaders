using UnityEngine;

internal class Edges
{
    public Vector3 TopEdge;
    public Vector3 LeftEdge;
    public Vector3 RightEdge;

    public Edges()
    {
        LeftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        RightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        TopEdge = Camera.main.ViewportToWorldPoint(Vector3.up);
    }

    private static Edges _instance;
    public static Edges Instance => _instance ??= new Edges();
}
