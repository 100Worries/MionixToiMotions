using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MionixToiMotions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            MainLoop();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        // See IMOTIONS API Programmer's guide page 28 and forth.
        // https://help.imotions.com/hc/en-us/articles/203045581-iMotions-API-Programming-Guide
        //
        // 1. Save this XML to file.
        // < EventSource Id = "GenericInput" Version = "1" Name = "GenericInput" >
        //      < Sample Id = "GenericInput" Name = "GenericInput" >
        //          < Field Id = "Value1" Range = "Variable" ></ Field >
        //          < Field Id = "Value2" Range = "Variable" ></ Field >
        //      </ Sample >
        //  </ EventSource >
        // 2. Load the file into IMOTIONS
        // 3. Enable API
        // 4. set receive method to UDP (non TCP)
        public static void MainLoop()
        {
            int lastshownsecond = -1;
            MionixData mData = new MionixData();

            while (true)
            {
                /*
                    SEND MIONIX DATA TO IMOTIONS
                */
                mData.Quality = 0.0f;
                mData.HeartRate = 0.0f;
                mData.AverageHeartRate = 0.0f;
                mData.MaxHeartRate = 0.0f;

                mData.RawHeartRate = 0.0f;
                mData.Galvanic = 0.0f;

                string UDPstring = "E;1;MionixNaos;2;0.0;;;MionixNaos;" + mData.Quality + ";"
                    + mData.HeartRate + ";"
                    + mData.AverageHeartRate + ";"
                    + mData.MaxHeartRate + ";"
                    + mData.RawHeartRate + ";"
                    + mData.Galvanic + ";"
                    + "\r\n";

                //new DTE based on metrics above
                SendUDPPacket("127.0.0.1", 8089, UDPstring, 1);

                /*
                    SEND MARKER TEXT AT DISCRETE TIME INTERVALS
                */
                if (lastshownsecond != DateTime.Now.Second)
                {
                    int currentSecond = DateTime.Now.Second;

                    // construct a UDP string with the above signals
                    // The prefix "M" lets IMOTIONS know that this is marker event.
                    string DiscreteTextEvent = "M;2;;;" + currentSecond + " Second;Marker Text with second counter " + currentSecond + ";D;\r\n";
                    SendUDPPacket("127.0.0.1", 8089, DiscreteTextEvent, 1);

                    lastshownsecond = currentSecond;
                    Console.WriteLine("API Marker sent to IMOTIONS " + DiscreteTextEvent);
                }

                // take a little break before generating a new sample
                // 10 milliseconds = 100hz signal
                System.Threading.Thread.Sleep(10);
            }
        }

        // <summary>
        // Sends a specified number of UDP packets to a host or IP Address.
        // </summary>
        // <param name="hostNameOrAddress">The host name or an IP Address to which the UDP packets will be sent.</param>
        // <param name="destinationPort">The destination port to which the UDP packets will be sent.</param>
        // <param name="data">The data to send in the UDP packet.</param>
        // <param name="count">The number of UDP packets to send.</param>
        // Thanks Ole Braunbaek Jensen for this function!
        private static void SendUDPPacket(string hostNameOrAddress, int destinationPort, string data, int count)
        {
            // Validate the destination port number
            if (destinationPort < 1 || destinationPort > 65535)
                throw new ArgumentOutOfRangeException("destinationPort", "Parameter destinationPort must be between 1 and 65,535.");

            // Resolve the host name to an IP Address
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostNameOrAddress);
            if (ipAddresses.Length == 0)
                throw new ArgumentException("Host name or address could not be resolved.", "hostNameOrAddress");

            // Use the first IP Address in the list
            IPAddress destination = ipAddresses[0];
            IPEndPoint endPoint = new IPEndPoint(destination, destinationPort);
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            // Send the packets
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            for (int i = 0; i < count; i++)
                socket.SendTo(buffer, endPoint);
            socket.Close();
        }

        // <summary>
        // Structure to hold the metrics we want to collect from the mouse
        // </summary>
        // <param name="quality">Quality of the PPG data being sent from the mouse.</param>
        // <param name="heartRate">Standing heart rate sent from the mouse.</param>
        // <param name="averageRate">Average heart rate sent from the mouse.</param>
        // <param name="maxRate">Max heart rate sent from the mouse.</param>
        // <param name="rawRate">Raw signal data from PPG sensor sent from mouse.</param>
        // <param name="galvanic">Current galvanic skin response signal being sent from the mouse.</param>
        public struct MionixData
        {
            //debating this struct and whether or not this is performance oriented enough for floats that change so frequently
            //https://softwareengineering.stackexchange.com/questions/188721/when-do-you-use-float-and-when-do-you-use-double/188725
            //https://stackoverflow.com/questions/3867331/why-is-struct-slower-than-float
            //https://hackaday.com/2018/03/02/unionize-your-variables-an-introduction-to-advanced-data-types-in-c/

            public float Quality { get; set; }
            public float HeartRate { get; set; }
            public float AverageHeartRate { get; set; }
            public float MaxHeartRate { get; set; }
            public float RawHeartRate { get; set; }
            public float Galvanic { get; set; }
        }
    }
}