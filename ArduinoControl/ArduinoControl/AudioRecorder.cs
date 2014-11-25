using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.DSP;
using CSCore.Codecs.WAV;


namespace ArduinoControl
{
    class AudioRecorder
    {
        private WaveInCaps selectedDevice;
        private WaveIn waveIn;

        public AudioRecorder()
        {
            selectedDevice = WaveIn.Devices[0];
        }

        public void record()
        {
            using (WasapiCapture capture = new WasapiCapture(false, CSCore.CoreAudioAPI.AudioClientShareMode.Exclusive, 0))
            {
                capture.Initialize();

                //create a wavewriter to write the data to
                using (WaveWriter w = new WaveWriter("dump.wav", capture.WaveFormat))
                {
                    //setup an eventhandler to receive the recorded data
                    capture.DataAvailable += (s, e) =>
                    {
                        //save the recorded audio
                        w.Write(e.Data, e.Offset, e.ByteCount);
                    };

                    //start recording
                    capture.Start();

                    Console.ReadKey();

                    //stop recording
                    capture.Stop();
                }
            }
        }

        ~AudioRecorder()
        {
        }
    }
}
