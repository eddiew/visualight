using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.DSP;
using MathNet.Numerics.IntegralTransforms;

namespace ArduinoControl
{
    class Program
    {
        const int BufferSamples = 4096;
        const int SampleSize = 4; // 2 shorts per sample
        const int BufferSize = BufferSamples * SampleSize;
        const int BRIGHTNESS_FACTOR = 1;

        static void Main(string[] args)
        {
            short[] buffer = new short[BufferSamples];
            int readOffset = 0;
            ArduinoControl arduino = new ArduinoControl("COM3");
            using (WasapiCapture capture = new WasapiCapture(false, CSCore.CoreAudioAPI.AudioClientShareMode.Exclusive, 0))
            {
                capture.Initialize();
                //setup an eventhandler to receive the recorded data
                capture.DataAvailable += (s, e) =>
                {
                    int nNewSamples = e.ByteCount / SampleSize;
                    int nSamplesToFill = Math.Min(nNewSamples, BufferSamples - readOffset);
                    //save the recorded audio
                    WriteSamples(e.Data, 0, buffer, readOffset, nSamplesToFill);
                    WriteSamples(e.Data, nSamplesToFill * SampleSize, buffer, 0, nNewSamples - nSamplesToFill);
                    // update circular buffer position
                    int newReadOffset = (readOffset + nNewSamples) % BufferSamples;
                    // update arduino twice per buffer fill
                    if ((newReadOffset >= BufferSamples / 2 && readOffset < BufferSamples / 2) ||
                        (newReadOffset < BufferSamples / 2 && readOffset >= BufferSamples / 2))
                    {
                        // Compute Fourier transform of most recent window of audio data
                        Complex[] fourierSamples = (buffer.Skip(newReadOffset).Concat(buffer.Take(newReadOffset))).Select(x => new Complex(x, 0)).ToArray();
                        Fourier.Radix2Forward(fourierSamples, FourierOptions.Default);
                        // TODO: do stuff
                        Visualize(fourierSamples, arduino);
                    }
                    readOffset = newReadOffset;
                };

                // Record & update arduino until keypress
                capture.Start();
                Console.ReadKey();
                capture.Stop();
            }
        }

        static void WriteSamples(byte[] sampleBytes, int sourceOffset, short[] dest, int destOffset, int samplesToWrite)
        {
            for (var i = 0; i < samplesToWrite; i++)
            {
                dest[destOffset + i] = BitConverter.ToInt16(sampleBytes, sourceOffset + i * 2 * sizeof(short)); // skip every other sample
            }
        }

        static void Visualize(Complex[] fourierOutput, ArduinoControl arduino)
        {
            Color vizColor = new Color(0, 0, 0);
            for (int i = 0; i < fourierOutput.Length / 2; i++) // divide by 2 b/c nyquist limit
            {
                float freq = 44100 * i / fourierOutput.Length;
                float power = (float)(fourierOutput[i].Real * fourierOutput[i].Real + fourierOutput[i].Imaginary * fourierOutput[i].Imaginary);
                float note = midiNote(freq);
                float h = note / 12;
                float s = 1;
                float l = power * BRIGHTNESS_FACTOR / (32768 * fourierOutput.Length);
                AddColors(vizColor, ColorFromHSL(h, s, l));
            }
            arduino.sendCommand(String.Format("r:{0} g:{1} b:{2}", vizColor.r, vizColor.g, vizColor.b));
        }

        static float midiNote(float frequency)
        {
            return (float)(69 + 12 * Math.Log(frequency / 440, 2));
        }

        struct Color
        {
            public float r, g, b;
            public Color(float r, float g, float b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }

        static Color ColorFromHSL(float h, float s, float l)
        {
            float r = 0, g = 0, b = 0;
            float temp1, temp2;

            if (l == 0)
            {
                r = g = b = 0;
            }
            else
            {
                if (s == 0)
                {
                    r = g = b = l;
                }
                else
                {
                    temp2 = ((l <= 0.5f) ? l * (1.0f + s) : l + s - (l * s));
                    temp1 = 2.0f * l - temp2;

                    float[] t3 = new float[] { h + 1.0f / 3.0f, h, h - 1.0f / 3.0f };
                    float[] clr = new float[] { 0, 0, 0 };
                    for (int i = 0; i < 3; i++)
                    {
                        if (t3[i] < 0)
                            t3[i] += 1.0f;
                        if (t3[i] > 1)
                            t3[i] -= 1.0f;

                        if (6.0f * t3[i] < 1.0f)
                            clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0f;
                        else if (2.0f * t3[i] < 1.0f)
                            clr[i] = temp2;
                        else if (3.0f * t3[i] < 2.0f)
                            clr[i] = (temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - t3[i]) * 6.0f);
                        else
                            clr[i] = temp1;
                    }
                    r = clr[0];
                    g = clr[1];
                    b = clr[2];
                }
            }

            return new Color(255 * r, 255 * g, 255 * b);
        }

        static void AddColors(Color a, Color b)
        {
            a.r += b.r;
            a.g += b.g;
            a.b += b.b;
        }
    }
}
