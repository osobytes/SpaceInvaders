using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : MonoBehaviour
{
    public float Speed = 5f;
    public float FireCooldownInSeconds = 0.5f;
    public GameObject BulletPrefab;
    private float MovementX = 0f;
    private FireCooldown FireCooldown;
    // Start is called before the first frame update
    void Start()
    {
        FireCooldown = new FireCooldown(FireCooldownInSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        if (MovementX != 0f)
        {
            var newPosition = transform.position + new Vector3(Speed * MovementX * Time.deltaTime, 0f, 0f);
            transform.position = ClampPosition(newPosition);
        }

        FireCooldown.Update();
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        // Clamp the position of the character so they do not go out of bounds
        position.x = Mathf.Clamp(position.x, Edges.Instance.LeftEdge.x, Edges.Instance.RightEdge.x);
        return position;
    }

    public void OnMove(InputValue value)
    {
        MovementX = value.Get<Vector2>().x;
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed && FireCooldown.CanFire)
        {
            // Fire Logic goes here.
            Instantiate(BulletPrefab, transform.position + new Vector3(0f, 0.3f, 0f), Quaternion.identity);
            FireCooldown.Cooldown();
        }
    }
}