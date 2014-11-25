using System;
using System.Collections.Generic;
using System.Drawing;
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
            Color vizColor = new Color();
            for (int i = 0; i < fourierOutput.Length / 2; i++) // divide by 2 b/c nyquist limit
            {
                float freq = 44100 * i / fourierOutput.Length;
                double power = fourierOutput[i].Real * fourierOutput[i].Real + fourierOutput[i].Imaginary * fourierOutput[i].Imaginary;
                float note = midiNote(freq) % 12;
            }
        }

        static float midiNote(float frequency)
        {
            return (float)(69 + 12 * Math.Log(frequency / 440, 2));
        }

        static Color ColorFromHSL()
        {

        }
    }
}
