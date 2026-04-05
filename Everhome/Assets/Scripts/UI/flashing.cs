using UnityEngine;

public class flashing : MonoBehaviour
{
    public GameObject target;
    float time = 0f;

    void Update()
    {
        time += Time.deltaTime;

        target.SetActive((int)time % 2 == 0);
    }
}
