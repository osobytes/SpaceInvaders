using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float Speed = 10f;
    public float Damage = 10f;

    // Update is called once per frame
    void Update()
    {
        var newPosition = transform.position + new Vector3(0f, Speed * Time.deltaTime * -1, 0f);
        transform.position = newPosition;

        if (transform.position.y < Edges.Values.Bottom)
        {
            Destroy(gameObject);
        }
    }
}
