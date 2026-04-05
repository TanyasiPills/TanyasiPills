using UnityEngine;

public class spin : MonoBehaviour
{
    void Update()
    {
        Vector3 rot = gameObject.transform.rotation.eulerAngles;
        float time = Time.deltaTime;
        Quaternion cur = Quaternion.Euler(rot.x + time*20, rot.y, rot.z + time*10);
        gameObject.transform.rotation = cur;
    }
}
