using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public Collider bodyCollider; 
    public Collider leftFootCollider;
    public Collider rightFootCollider;

    void Awake()
    {
        // 1. Ignore collision between the body and the left foot
        if (bodyCollider != null && leftFootCollider != null)
        {
            Physics.IgnoreCollision(bodyCollider, leftFootCollider, true);
        }
        
        // 2. Ignore collision between the body and the right foot
        if (bodyCollider != null && rightFootCollider != null)
        {
            Physics.IgnoreCollision(bodyCollider, rightFootCollider, true);
        }
        
        
        if (leftFootCollider != null && rightFootCollider != null)
        {
            Physics.IgnoreCollision(leftFootCollider, rightFootCollider, true);
        }

    }
}
