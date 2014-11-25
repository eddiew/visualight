using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ArduinoControl
{
    class ArduinoControl
    {
        SerialPort port;
        public ArduinoControl(string portName, int baudRate = 9600)
        {
            port = new SerialPort(portName, baudRate);
            port.Open();
        }

        public void sendCommand(string command)
        {
            port.Write(command + '\n');
        }

        ~ArduinoControl()
        {
            port.Close();
        }
    }
}
