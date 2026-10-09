using System.IO;
using UnityEditor;
using UnityEngine;

public class AudioGenerator
{
    [MenuItem("Tools/Pasit/Generate Default Audio Files")]
    public static void GenerateDefaultAudioFiles()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Audio"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Audio");
        }

        string bgmPath = "Assets/Resources/Audio/BGM_Default.wav";
        string itemSfxPath = "Assets/Resources/Audio/SFX_Item.wav";
        string obstacleSfxPath = "Assets/Resources/Audio/SFX_Obstacle.wav";

        CreateWavFile(bgmPath, GenerateBgmSamples(44100, 4.0f));
        CreateWavFile(itemSfxPath, GenerateItemSamples(44100, 0.3f));
        CreateWavFile(obstacleSfxPath, GenerateObstacleSamples(44100, 0.4f));

        AssetDatabase.Refresh();
        Debug.Log("Audio files successfully generated in Assets/Resources/Audio/!");
    }

    private static float[] GenerateBgmSamples(int sampleRate, float duration)
    {
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        // 4 chords: Cmaj, Gmaj, Amin, Fmaj (arpeggio loop)
        float[][] chords = new float[][] {
            new float[] { 261.63f, 329.63f, 392.00f, 523.25f }, // C
            new float[] { 196.00f, 246.94f, 293.66f, 392.00f }, // G
            new float[] { 220.00f, 261.63f, 329.63f, 440.00f }, // Am
            new float[] { 174.61f, 220.00f, 261.63f, 349.23f }  // F
        };

        float chordDuration = duration / chords.Length;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            int chordIndex = (int)(t / chordDuration) % chords.Length;
            float chordT = t % chordDuration;

            float[] notes = chords[chordIndex];
            int noteIndex = (int)(chordT / (chordDuration / 4)) % 4;
            float freq = notes[noteIndex];

            // Soft synth wave with subtle harmonic
            float val = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.2f
                      + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.05f;

            // Envelope per note
            float noteT = chordT % (chordDuration / 4);
            float env = Mathf.Exp(-noteT * 3.5f);

            samples[i] = val * env;
        }

        return samples;
    }

    private static float[] GenerateItemSamples(int sampleRate, float duration)
    {
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            // Bell chime glissando from 880Hz to 1320Hz
            float freq = Mathf.Lerp(880f, 1320f, t / duration);
            float val = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.35f
                      + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.15f;
            float env = Mathf.Exp(-t * 10f); // fast decay
            samples[i] = val * env;
        }

        return samples;
    }

    private static float[] GenerateObstacleSamples(int sampleRate, float duration)
    {
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            // Low thud/buzz decaying from 180Hz to 60Hz
            float freq = Mathf.Lerp(180f, 60f, t / duration);
            float val = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
            // Add slight grit
            val += (Random.value * 2f - 1f) * 0.08f;
            float env = Mathf.Exp(-t * 8f);
            samples[i] = val * env;
        }

        return samples;
    }

    private static void CreateWavFile(string filePath, float[] samples)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            int sampleRate = 44100;
            short channels = 1;
            short bitsPerSample = 16;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            int subChunk2Size = samples.Length * (bitsPerSample / 8);
            int chunkSize = 36 + subChunk2Size;

            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunkSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // Subchunk1Size (16 for PCM)
            bw.Write((short)1); // AudioFormat (1 for PCM)
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(bitsPerSample);

            // data subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(subChunk2Size);

            // Audio data
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
                bw.Write(sample);
            }
        }
    }
}
