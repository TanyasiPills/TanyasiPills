using UnityEngine;

public class SpeakerBody : MonoBehaviour
{
    public AP ap = null;

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (ap != null)
            ap.PopSamples(data, channels);
        else
            for (int i = 0; i < data.Length; i++)data[i] = 0f;
    }
}
