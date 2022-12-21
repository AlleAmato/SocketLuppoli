using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.Serial;

namespace NModBusApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //configurare la porta seriale dell'applicazione
            using (SerialPort port = new SerialPort("COM2"))
            {
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = Parity.Even;
                port.StopBits = StopBits.One;
                //parametri che devono essere sempre così
                port.Open();

                //creare l'oggetto modbusfactory
                var factory = new ModbusFactory();

                //creare 
                var bus = factory.CreateRtuMaster(port);

                ushort[] lettura = await bus.ReadHoldingRegistersAsync(1, 3, 1);
                //parametri del motodo => slave address (porta), indirizzo slave di partenza, numero di indirizzi da legere

                ushort temp = lettura[0];
                bool accendi = temp < 200 ? true : false;
                await bus.WriteSingleCoilAsync(1, 5, accendi);
                //parametri del motodo => indirizzo slave, indirizzo coil, variabile booleana
            }
        }
    }
}
