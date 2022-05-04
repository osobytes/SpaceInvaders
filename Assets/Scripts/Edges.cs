using UnityEngine;

internal class Edges
{
    public float TopEdge;
    public float LeftEdge;
    public float RightEdge;
    public float BottomEdge;

    public Edges()
    {
        LeftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
        RightEdge = Camera.main.ViewportToWorldPoint(Vector3.right).x;
        TopEdge = Camera.main.ViewportToWorldPoint(Vector3.up).y;
        BottomEdge = Camera.main.ViewportToWorldPoint(Vector3.down).y;
    }

    private static Edges _instance;
    public static Edges Instance => _instance ??= new Edges();
}
