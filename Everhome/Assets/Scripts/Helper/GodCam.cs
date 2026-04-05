using System.Collections;
using UnityEngine;

public class GodCam : MonoBehaviour
{
    public float duration = 10f;

    public IEnumerator MoveCamera(Transform target, PlayerMovement script)
    {
        float t = 0f;

        Vector3 originPos = transform.position;
        Quaternion originRot = transform.rotation;

        while (t < 1f)
        {

            t += Time.deltaTime / duration;

            Debug.Log(t);

            transform.position = Vector3.Slerp(originPos, target.position, t);
            transform.rotation = Quaternion.Slerp(originRot, target.rotation, t);

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        script.OnGodCamFinish();

        transform.position = originPos;
        transform.rotation = originRot;
    }
}
