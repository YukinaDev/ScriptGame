using UnityEngine;

public class CollisionDebug : MonoBehaviour
{
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("[COLLISION] Hit: " + hit.gameObject.name + " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer) + " | IsTrigger: " + hit.collider.isTrigger);
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[TRIGGER ENTER] " + other.gameObject.name);
    }
    
    void OnDrawGizmos()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + cc.center, cc.radius);
            Gizmos.DrawWireCube(transform.position + cc.center, new Vector3(cc.radius * 2, cc.height, cc.radius * 2));
        }
    }
}
