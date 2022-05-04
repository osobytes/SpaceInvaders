using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : MonoBehaviour
{
    public float Speed = 5f;
    public float FireCooldownInSeconds = 0.5f;
    public GameObject BulletPrefab;
    public float Health = 100;
    private float MovementX = 0f;
    private FireCooldown FireCooldown;
    private HealthController HealthCtl;
    // Start is called before the first frame update
    void Start()
    {
        FireCooldown = new FireCooldown();
        HealthCtl = new HealthController(Health);
        HealthCtl.OnHealthDepleted = OnHealthDepleted;
    }

    // Update is called once per frame
    void Update()
    {
        if (MovementX != 0f)
        {
            var newPosition = transform.position + new Vector3(Speed * MovementX * Time.deltaTime, 0f, 0f);

            // We need to Clamp position in order to not allow the player to go outside of the screen.
            transform.position = ClampPosition(newPosition);
        }

        FireCooldown.Cooldown();
    }

    void OnHealthDepleted()
    {
        Debug.Log("Health has depleted");
        Destroy(gameObject);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        // Clamp the position of the character so they do not go out of bounds
        position.x = Mathf.Clamp(position.x, Edges.Instance.LeftEdge, Edges.Instance.RightEdge);
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
            FireCooldown.SetCooldown(FireCooldownInSeconds);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<EnemyBullet>() != null)
        {
            Destroy(collision.gameObject);
        }
    }
}