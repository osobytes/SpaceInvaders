using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed = 10f;

    // Update is called once per frame
    void Update()
    {
        var newPosition = transform.position + new Vector3(0f, Speed * Time.deltaTime, 0f);
        transform.position = newPosition;

        if(transform.position.y > Edges.Instance.TopEdge)
        {
            Destroy(gameObject);
        }
    }
}
