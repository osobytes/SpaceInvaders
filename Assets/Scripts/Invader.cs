using System;
using UnityEngine;

public class Invader : MonoBehaviour
{
    public GameObject EnemyBulletPrefab;
    public float FireCooldownInSeconds;
    public Action<int, int> OnDestroyed;
    private int Row;
    private int Column;
    private TweenMovement Movement;
    public void SetRowAndColumn(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public void SetMoveX(float move, float delay, float speed)
    {
        Movement.SetMove(new Vector3(move, 0f, 0f), delay, speed);
    }

    public void SetMoveY(float move, float delay, float speed)
    {
        Movement.SetMove(new Vector3(0f, move, 0f), delay, speed);
    }

    public (int Row, int Column) GetLineAndColumn()
    {
        return (Row, Column);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var delta = Movement.GetMoveDelta();
        if(delta == Vector3.zero)
        {
            return;
        }
        transform.position = transform.position + delta;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var heroBullet = collision.gameObject.GetComponent<Bullet>();
        if(heroBullet != null)
        {
            Destroy(heroBullet.gameObject); // Destroy the bullet.
            OnDestroyed?.Invoke(Row, Column);
            Destroy(gameObject); // Destroy the invader.
        }
    }

    public void FireProjectile()
    {
        Instantiate(EnemyBulletPrefab, transform.position + new Vector3(0f, 0.3f * -1, 0f), Quaternion.identity);
    }
}

public struct TweenMovement {
    private float x;
    private float y;
    private float cumulativeX;
    private float cumulativeY;
    private float Speed;
    private float Delay;

    public void SetMove(Vector3 delta, float delay, float speed)
    {
        x = delta.x;
        y = delta.y;
        cumulativeX = 0f;
        cumulativeY = 0f;
        Delay = delay;
        Speed = speed;
    }

    public Vector3 GetMoveDelta()
    {
        if (x == 0f && y == 0f)
        {
            return Vector3.zero;
        }

        if (Delay > 0f)
        {
            Delay = Mathf.Max(0f, Delay - Time.deltaTime);
            return Vector3.zero;
        }

        var xDelta = 0f;
        var yDelta = 0f;

        // Whether it's moving to the left or right.
        // We use positives for right, and negatives for left.
        var xDirection = x < 0 ? -1 : 1;
        var yDirection = y < 0 ? -1 : 1;
        xDelta = Mathf.Min(Mathf.Abs(x) - Mathf.Abs(cumulativeX), Speed * Time.deltaTime) * xDirection;
        yDelta = Mathf.Min(Mathf.Abs(y) - Mathf.Abs(cumulativeY), Speed * Time.deltaTime) * yDirection;

        cumulativeX += xDelta;
        cumulativeY += yDelta;

        if (cumulativeX == x)
        {
            x = 0f;
        }
        if (cumulativeY == y)
        {
            y = 0f;
        }

        return new Vector3(xDelta, yDelta);
    }
}
