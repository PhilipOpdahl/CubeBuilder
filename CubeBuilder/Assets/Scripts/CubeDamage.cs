using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDamage : MonoBehaviour
{
    public float damageThreshold = 10f;
    public float destroyThreshold = 50f;
    private float damage = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && collision.relativeVelocity.magnitude > damageThreshold)
        {
            damage += collision.relativeVelocity.magnitude;

            if (damage >= destroyThreshold)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
