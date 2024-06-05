namespace SharedLibrary;

using System;

[Serializable]
public class DataObject
{
    public string ClientName { get; set; }
    public DateTime Timestamp { get; set; }
    public int Number { get; set; }
}

