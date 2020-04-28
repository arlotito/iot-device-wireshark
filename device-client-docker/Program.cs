// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace DeviceClientSas
{
    class Program
    {
        static private DeviceClient deviceClient;
        
        // For this sample either:
        // - set the IOTHUB_CONN_STRING_CSHARP environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        private static string deviceConnectionString = "";

        /// <summary>
        /// Creates a string of specified length with random chars 
        /// (from "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") 
        /// </summary>
        /// <param name="length">Number of chars</param>
        /// <returns>the string</returns>
        static String RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        /// <summary>
        /// Sends telemetry messages with the following structure:
        /// 
        ///     var telemetryDataPoint = new
        ///     {
        ///         var1 = RandomString(stringLength)
        ///     };
        /// 
        /// The 'stringLength' is automatically increased starting from 'stringLengthMin'
        /// to 'stringLengthMax' with step 'step'. 
        /// 
        /// Transmission stops when either 'stringLengthMax' or 'maxNumOfMessages' are reached.
        /// 
        /// Messages are sent with an interval='intervalMilliseconds'.
        /// </summary>
        /// <param name="stringLengthMin">Num of chars to start with</param>
        /// <param name="stringLengthMax">Num of chars to end with</param>
        /// <param name="step">Num of chars to be added at each transmission</param>
        /// <param name="maxNumOfMessages">Max number of messages to be sent</param>
        /// <param name="intervalMilliseconds">Interval in milliseconds between one message and the next</param>
        /// <returns></returns>
        private static async void SendDeviceToCloudMessagesVariableLengthAsync(
            int stringLengthMin, 
            int stringLengthMax,
            int step,
            int maxNumOfMessages,
            int intervalMilliseconds)
        {
            int length = stringLengthMin;
            int numOfMessages = 0;

            while ( (length < stringLengthMax) && (numOfMessages < maxNumOfMessages))
            {
                // Create JSON message
                var telemetryDataPoint = new
                {
                    var1 = RandomString(length)
                };


                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var payload = Encoding.ASCII.GetBytes(messageString);
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} > {numOfMessages}, length: {payload.Length}, payload: {messageString}");
                var message = new Message(payload);

                // Send the telemetry message
                await deviceClient.SendEventAsync(message);
                //Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                numOfMessages++;
                length+=step;

                await Task.Delay(intervalMilliseconds);
            }

            Console.WriteLine("Transmission completed!");
        }

        static void Main(string[] args)
        {
            // command line
            // dotnet run protocol stringLengthMin stringLengthMax step maxNumOfMessages intervalMilliseconds
            //
            // where protocol = mqtt|amqp|https
            
            if (string.IsNullOrWhiteSpace(deviceConnectionString))
            {
                deviceConnectionString = Environment.GetEnvironmentVariable("IOTHUB_CONN_STRING_CSHARP");
                
                if (deviceConnectionString == null)
                {
                    Console.WriteLine("ERROR: env var 'IOTHUB_CONN_STRING_CSHARP' must contain connection string");
                    Environment.Exit(1);
                }
                
                //removes quotes if any
                deviceConnectionString = deviceConnectionString.Replace("\"", "");
            }

            TransportType protocol = TransportType.Mqtt;
            var protocol_str = new String(args[0]).ToLower();

            switch (protocol_str)
            {
                case "mqtt":
                    protocol = TransportType.Mqtt;
                    break;

                case "amqp":
                    protocol = TransportType.Amqp;
                    break;

                case "https":
                    protocol = TransportType.Http1;
                    break;
            }

            // creates device client
            Console.WriteLine("Creates device client");
            deviceClient = DeviceClient.CreateFromConnectionString(
                deviceConnectionString,
                protocol
            );

            //wait for user to press a key
            Console.WriteLine("Press ENTER to connect to the IoT HUB...");
            Console.ReadLine();
            deviceClient.OpenAsync();

            //wait for user to press a key
            Console.WriteLine("Press ENTER to send messages...");
            Console.ReadLine();
            Console.WriteLine("Press ctrl-C to stop!");
            
            // sends message
            SendDeviceToCloudMessagesVariableLengthAsync(
                int.Parse(args[1]), //length min
                int.Parse(args[2]), //length max
                int.Parse(args[3]), //step
                int.Parse(args[4]), //maxNumOfMessages
                int.Parse(args[5])  //milliseconds
            );

            //wait for user to press a key
            Console.ReadLine();
        }
    }
}