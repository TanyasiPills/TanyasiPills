using Concentus.Enums;
using Concentus.Structs;
using UnityEngine;
using NAudio.Wave;
using System;
using UnityEngine.InputSystem.LowLevel;

public class AudioRecord : MonoBehaviour
{
    public static OpusEncoder encoder;
    public static OpusDecoder decoder;
    static WaveInEvent waveIn;

    static public int freq = 48000;
    static public int chunkSize;

    public Transform speaker;
    private AudioPlayer player;

    //[Range(0, 0.5f)]
    public float threshold = 0.1f; 

    void Start()
    {
        encoder = new OpusEncoder(freq, 1, OpusApplication.OPUS_APPLICATION_VOIP);
        decoder = new OpusDecoder(freq, 1);

        chunkSize = freq / 100;

        player = speaker.GetComponent<AudioPlayer>();

        waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 0;
        waveIn.BufferMilliseconds = chunkSize / (freq / 1000);
        waveIn.WaveFormat = new WaveFormat(freq, 16, 1);
        waveIn.DataAvailable += OnDataAvailable;
        waveIn.StartRecording();
    }

    private void OnDestroy()
    {
        waveIn.DataAvailable -= OnDataAvailable;
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            int sampleCount = e.BytesRecorded / 2;
            short[] samples = new short[sampleCount];
            for (int i = 0; i < sampleCount; i++)
                samples[i] = BitConverter.ToInt16(e.Buffer, i * 2);

            if (!IsActive(samples, threshold)) return;

            byte[] compressed = new byte[chunkSize * 2];

            ReadOnlySpan<short> pcm = samples.AsSpan(0, chunkSize);

            Span<byte> outSpan = compressed.AsSpan();

            int compressedLength = encoder.Encode(pcm, chunkSize, outSpan, compressed.Length);
            player.PushSamples(compressed, compressedLength);
  
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Debug.Log(ex.Data);
            Debug.Log(ex.StackTrace);
        }
    }

    private bool IsActive(short[] data, float scale)
    {
        double energy = 0;

        for (int i = 0; i < data.Length; i++)
        {
            float s = data[i];
            energy += s * s;
        }

        double rms = Math.Sqrt(energy / data.Length);

        return rms > threshold;
    }
}
