using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SharedLibrary;

class Server
{
    private static TcpListener listener;
    private static bool isRunning = true;

    static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, 8000);
        listener.Start();
        Console.WriteLine("Server started...");

        while (isRunning)
        {
            var client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected...");
            var clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private static void HandleClient(object clientObj)
    {
        var client = (TcpClient)clientObj;
        var stream = client.GetStream();
        IFormatter formatter = new BinaryFormatter();

        while (true)
        {
            try
            {
                // Odbierz obiekt
                var dataObject = (DataObject)formatter.Deserialize(stream);
                Console.WriteLine($"Received from {dataObject.ClientName} at {dataObject.Timestamp}: {dataObject.Number}");

                // Modyfikuj obiekt
                dataObject.Number += 1;
                dataObject.Timestamp = DateTime.Now;
                Console.WriteLine($"Modified: {dataObject.Number}");

                // Wyślij zmodyfikowany obiekt z powrotem do klienta
                formatter.Serialize(stream, dataObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }

        client.Close();
    }
}
