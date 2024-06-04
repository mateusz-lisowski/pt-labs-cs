using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SharedLibrary;

class Client
{
    private static TcpClient client;
    private static NetworkStream stream;
    private static IFormatter formatter;
    private static string clientName;

    static void Main(string[] args)
    {
        Console.Write("Enter your client name: ");
        clientName = Console.ReadLine();

        client = new TcpClient("localhost", 8000);
        stream = client.GetStream();
        formatter = new BinaryFormatter();
        Console.WriteLine("Connected to server...");

        var dataObject = new DataObject { ClientName = clientName, Number = 0, Timestamp = DateTime.Now };

        var sendThread = new Thread(SendData);
        sendThread.Start(dataObject);

        while (true)
        {
            try
            {
                // Odbierz zmodyfikowany obiekt od serwera
                var modifiedDataObject = (DataObject)formatter.Deserialize(stream);
                Console.WriteLine($"Received modified data from {modifiedDataObject.ClientName} at {modifiedDataObject.Timestamp}: {modifiedDataObject.Number}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }

        client.Close();
    }

    private static void SendData(object dataObj)
    {
        var dataObject = (DataObject)dataObj;

        while (true)
        {
            try
            {
                dataObject.Timestamp = DateTime.Now;

                // Serializuj i wyślij obiekt do serwera
                formatter.Serialize(stream, dataObject);
                Console.WriteLine($"Sent: {dataObject.Number} at {dataObject.Timestamp}");

                // Czekaj przed wysłaniem następnych danych
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }
    }
}
