using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConsoleFileExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Nie podano ścieżki do katalogu.");
                return;
            }

            string directoryPath = args[0];
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                Console.WriteLine("Podany katalog nie istnieje.");
                return;
            }

            // Wyświetlanie zawartości katalogu
            Console.WriteLine($"Zawartość katalogu: {directoryPath}");
            DisplayDirectoryContents(directoryInfo, 0);

            // Najstarszy plik
            DateTime oldestFileDate = GetOldestFileDate(directoryInfo);
            Console.WriteLine($"\nNajstarszy plik: {oldestFileDate}");

            // Ładowanie elementów katalogu do uporządkowanej kolekcji
            var filesCollection = LoadDirectoryElements(directoryInfo);
            Console.WriteLine("\nElementy katalogu załadowane do uporządkowanej kolekcji:");
            foreach (var entry in filesCollection)
            {
                Console.WriteLine($"{entry.Key} -> {entry.Value}");
            }

            // Serializacja i deserializacja binarna kolekcji
            SerializeAndDeserializeCollection(filesCollection);
        }

        static void DisplayDirectoryContents(DirectoryInfo directory, int indentLevel)
        {
            foreach (var file in directory.GetFiles())
            {
                Console.WriteLine($"{new string(' ', indentLevel * 4)}{file.Name} {file.Length} bajtów {GetFileAttributes(file.Attributes)}");
            }

            foreach (var subDir in directory.GetDirectories())
            {
                Console.WriteLine($"{new string(' ', indentLevel * 4)}{subDir.Name} ({subDir.GetFiles().Length}) {GetFileAttributes(subDir.Attributes)}");
                DisplayDirectoryContents(subDir, indentLevel + 1);
            }
        }

        static string GetFileAttributes(FileAttributes attributes)
        {
            return ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "r" : "-") +
                   ((attributes & FileAttributes.Archive) == FileAttributes.Archive ? "a" : "-") +
                   ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "h" : "-") +
                   ((attributes & FileAttributes.System) == FileAttributes.System ? "s" : "-");
        }

        static DateTime GetOldestFileDate(DirectoryInfo directory)
        {
            DateTime oldestDate = DateTime.MaxValue;

            foreach (var file in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.LastWriteTime < oldestDate)
                {
                    oldestDate = file.LastWriteTime;
                }
            }

            return oldestDate;
        }

        static SortedDictionary<string, long> LoadDirectoryElements(DirectoryInfo directory)
        {
            var comparer = new CustomComparer();

            var filesCollection = new SortedDictionary<string, long>(comparer);

            foreach (var file in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                filesCollection.Add(file.FullName, file.Length);
            }

            return filesCollection;
        }


        static void SerializeAndDeserializeCollection(SortedDictionary<string, long> collection)
        {
            string fileName = "collection.bin";

            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, collection);
                Console.WriteLine($"\nKolekcja została zserializowana do pliku {fileName}");
            }

            SortedDictionary<string, long> deserializedCollection;
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                deserializedCollection = (SortedDictionary<string, long>)formatter.Deserialize(stream);
            }

            Console.WriteLine("\nDeserializowana kolekcja:");
            foreach (var entry in deserializedCollection)
            {
                Console.WriteLine($"{entry.Key} -> {entry.Value}");
            }
        }

        [Serializable]
        class CustomComparer : IComparer<string>, ISerializable
        {
            public int Compare(string x, string y)
            {
                if (x.Length != y.Length)
                    return x.Length.CompareTo(y.Length);
                else
                    return x.CompareTo(y);
            }

            // Konstruktor bezargumentowy
            public CustomComparer()
            {
                // Konstruktor bezargumentowy jest wymagany dla tworzenia nowych instancji klasy CustomComparer
            }

            // Konstruktor wymagany dla ISerializable
            protected CustomComparer(SerializationInfo info, StreamingContext context)
            {
                // Nie jest wymagane przetwarzanie danych przy deserializacji
            }

            // Metoda wymagana dla ISerializable
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                // Nie ma żadnych danych do serializacji, więc ta metoda może pozostać pusta
            }
        }

    }
}
