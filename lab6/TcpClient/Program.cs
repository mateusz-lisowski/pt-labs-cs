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

    static void Main(string[] args)
    {
        client = new TcpClient("localhost", 8000);
        stream = client.GetStream();
        formatter = new BinaryFormatter();
        Console.WriteLine("Connected to server...");

        var dataObject = new DataObject { Number = 0 };

        var sendThread = new Thread(SendData);
        sendThread.Start(dataObject);

        while (true)
        {
            try
            {
                // Odbierz zmodyfikowany obiekt od serwera
                var modifiedDataObject = (DataObject)formatter.Deserialize(stream);
                Console.WriteLine($"Received modified data: {modifiedDataObject.Number}");
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
                // Serializuj i wyślij obiekt do serwera
                formatter.Serialize(stream, dataObject);
                Console.WriteLine($"Sent: {dataObject.Number}");

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
