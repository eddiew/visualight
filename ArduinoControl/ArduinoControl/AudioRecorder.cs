using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.DSP;


namespace ArduinoControl
{
    class AudioRecorder
    {
        private WaveInCaps selectedDevice;
        private WaveIn waveIn;

        public AudioRecorder()
        {
        }

        ~AudioRecorder()
        {
        }
    }
}
