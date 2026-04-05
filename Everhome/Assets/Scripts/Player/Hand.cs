using Unity.Netcode;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    LayerMask layerMask;
    PlayerMovement playerScript;

    void Start()
    {
        if (!IsOwner) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerMovement>();
        layerMask = playerScript.layerMask;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        int layer = other.gameObject.layer;
        if ((layerMask & (1 << layer)) != 0)
        {
            Transform target = other.transform.parent;

            Vector3 worldPoint = other.ClosestPoint(transform.position);
            Vector3 localPoint = target.InverseTransformPoint(worldPoint);

            playerScript.Pickup(target.gameObject, gameObject, localPoint);
        }
    }
}
