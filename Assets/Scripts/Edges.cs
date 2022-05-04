using UnityEngine;

internal class Edges
{
    public readonly float Top;
    public readonly float Left;
    public readonly float Right;
    public readonly float Bottom;

    public Edges()
    {
        Left = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
        Right = Camera.main.ViewportToWorldPoint(Vector3.right).x;
        Top = Camera.main.ViewportToWorldPoint(Vector3.up).y;
        Bottom = Camera.main.ViewportToWorldPoint(Vector3.down).y;
    }

    private static Edges _instance;
    public static Edges Values => _instance ??= new Edges();
}
