using UnityEngine;

public class Hand : MonoBehaviour
{
    LayerMask layerMask;
    PlayerMovement playerScript;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerMovement>();
        layerMask = playerScript.layerMask;
    }

    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        if ((layerMask & (1 << layer)) != 0)
        {
            Vector3 worldPoint = other.ClosestPoint(transform.position);
            Vector3 localPoint = other.transform.InverseTransformPoint(worldPoint);
            playerScript.Pickup(other.gameObject, gameObject, localPoint);
        }
    }
}
