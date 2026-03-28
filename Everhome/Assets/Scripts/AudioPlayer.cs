using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;
    float[] decodeBuffer = new float[AudioRecord.chunkSize];
    float[] oriboriBuffer = new float[AudioRecord.freq];

    int readpos = 0;
    int writepos = 0;


    private void OnAudioFilterRead(float[] data, int channels)
    {
        int localRead = readpos;
        int localWrite = writepos;

        int available = (localWrite - localRead + oriboriBuffer.Length) % oriboriBuffer.Length;

        if (available < data.Length / channels * 20)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = 0f;
            return;
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = oriboriBuffer[localRead];
            localRead = (localRead + 1) % oriboriBuffer.Length;

            for (int c = 0; c < channels; c++)
                data[i + c] = sample;
        }

        readpos = localRead;
    }

    public void PushSamples(byte[] newSamples, int length)
    {
        int decodedSamples = AudioRecord.decoder.Decode(newSamples, 0, length, decodeBuffer, 0, decodeBuffer.Length);

        for (int i = 0; i < decodedSamples; i++)
        {
            int next = (writepos + 1) % oriboriBuffer.Length;

            if (next == readpos)
                readpos = (readpos + 1) % oriboriBuffer.Length;

            oriboriBuffer[writepos] = decodeBuffer[i];
            writepos = next;
        }
    }
}