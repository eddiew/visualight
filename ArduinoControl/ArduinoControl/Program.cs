using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.DSP;

namespace ArduinoControl
{
    class Program
    {
        const int BufferSize = 44100 * sizeof(short) * 2 / 10;
        static short[] buffer = new short[BufferSize];
        static int ReadOffset = 0;

        static void Main(string[] args)
        {
            ArduinoControl arduino = new ArduinoControl("COM3");

            //while(true)
            //{
            //    string s = Console.ReadLine();
            //    if (s.Equals("q"))
            //    {
            //        break;
            //    }
            //    arduino.sendCommand(s);
            //}
            using (WasapiCapture capture = new WasapiCapture(false, CSCore.CoreAudioAPI.AudioClientShareMode.Exclusive, 0))
            {
                capture.Initialize();
                //setup an eventhandler to receive the recorded data
                capture.DataAvailable += (s, e) =>
                {
                    int bytesToWrite = Math.Min(e.ByteCount, BufferSize - ReadOffset);
                    //save the recorded audio
                    w.Write(e.Data, e.Offset, bytesToWrite);
                    w.Write(e.Data.Skip(bytesToWrite).ToArray(), 0, e.ByteCount - bytesToWrite);
                    int newReadOffset = (ReadOffset + e.ByteCount) % BufferSize;
                    if ((newReadOffset >= BufferSize / 2 && ReadOffset < BufferSize / 2) ||
                        (newReadOffset < BufferSize / 2 && ReadOffset >= BufferSize / 2))
                    {

                    }
                    ReadOffset = newReadOffset;
                };

                //start recording
                capture.Start();

                Console.ReadKey();

                //stop recording
                capture.Stop();
            }
        }

        void fourierTransform(MemoryStream )
    }
}
