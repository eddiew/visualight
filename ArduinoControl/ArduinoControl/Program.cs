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
    class Program
    {
        private WaveInCaps selectedDevice;
        private WaveIn waveIn;

        static void Main(string[] args)
        {
            ArduinoControl arduino = new ArduinoControl("COM3");
            AudioRecorder recorder = new AudioRecorder();
            foreach (var device in WaveIn.Devices)
            {
                Console.WriteLine(device.Name);
            }
            recorder.record();
            //while(true)
            //{
            //    string s = Console.ReadLine();
            //    if (s.Equals("q"))
            //    {
            //        break;
            //    }
            //    arduino.sendCommand(s);
            //}
        }
    }
}
