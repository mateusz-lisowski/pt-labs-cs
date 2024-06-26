﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace LINQQueries
{
    [Serializable]
    public class Car
    {
        [XmlElement("model")]
        public string Model { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlElement("engine")]
        public Engine CarEngine { get; set; }

        // Domyślny konstruktor dla Serializera
        public Car()
        {
        }

        public Car(string model, Engine engine, int year)
        {
            Model = model;
            CarEngine = engine;
            Year = year;
        }
    }

    [Serializable]
    public class Engine
    {
        [XmlElement("displacement")]
        public double Displacement { get; set; }

        [XmlElement("horsePower")]
        public double HorsePower { get; set; }

        [XmlElement("model")]
        public string Model { get; set; }

        // Domyślny konstruktor dla Serializera
        public Engine()
        {
        }

        public Engine(double displacement, double horsePower, string model)
        {
            Displacement = displacement;
            HorsePower = horsePower;
            Model = model;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Car> myCars = new List<Car>()
            {
                new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
                new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
                new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
                new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
                new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
                new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
                new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
                new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
                new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };

            // Zapytanie LINQ #1
            var query1 = myCars.Where(c => c.Model == "A6")
                               .Select(c => new
                               {
                                   engineType = c.CarEngine.Model == "TDI" ? "diesel" : "petrol",
                                   hppl = c.CarEngine.HorsePower / c.CarEngine.Displacement
                               });

            // Zapytanie LINQ #2
            var query2 = query1.GroupBy(c => c.engineType)
                                .Select(g => new
                                {
                                    engineType = g.Key,
                                    avgHPPL = g.Average(c => c.hppl)
                                });

            foreach (var item in query1)
            {
                Console.WriteLine($"Engine Type: {item.engineType}, HPPL: {item.hppl}");
            }

            foreach (var item in query2)
            {
                Console.WriteLine($"Engine Type: {item.engineType}, Avg HPPL: {item.avgHPPL}");
            }

            // Serializacja do XML
            SerializeToXml(myCars, "CarsCollection.xml");

            // Deserializacja z XML
            List<Car> deserializedCars = DeserializeFromXml("CarsCollection.xml");

            XElement rootNode = XElement.Load("CarsCollection.xml");

            // Wyrażenie XPath #1
            double avgHP = (double)rootNode.XPathEvaluate("sum(//car[engine/@model != 'TDI']/engine/horsePower) div count(//car[engine/@model != 'TDI'])");

            // Wyrażenie XPath #2
            var models = rootNode.XPathSelectElements("//car/model").Select(m => m.Value).Distinct();

            Console.WriteLine($"Średnia moc samochodów o silnikach innych niż TDI: {avgHP}");
            Console.WriteLine("Modele samochodów bez powtórzeń:");
            foreach (var model in models)
            {
                Console.WriteLine(model);
            }

            GenerateXhtmlDocument(myCars, "CarsTable.html");

            XDocument xmlDocument = XDocument.Load("CarsCollection.xml");

            // Zmiana nazwy elementu horsePower na hp
            foreach (var element in xmlDocument.Descendants("horsePower").ToList())
            {
                element.Name = "hp";
            }

            // Zmiana elementu year na atrybut model
            foreach (var carElement in xmlDocument.Descendants("car"))
            {
                var modelElement = carElement.Element("model");
                var yearElement = carElement.Element("year");

                if (modelElement != null && yearElement != null)
                {
                    modelElement.SetAttributeValue("year", yearElement.Value);
                    yearElement.Remove();
                }
            }

            xmlDocument.Save("ModifiedCarsCollection.xml");
            Console.WriteLine("Dokument XML został zmodyfikowany zgodnie z wymaganiami.");
        }

        static void SerializeToXml(List<Car> cars, string fileName)
        {
            // Konfiguracja serializera
            XmlSerializer serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", ""); // Usunięcie domyślnego namespace'u

            // Serializacja
            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(stream, cars, namespaces);
            }

            XDocument xmlDocument = XDocument.Load(fileName);

            // Zmiana nazwy elementu horsePower na hp
            foreach (var element in xmlDocument.Descendants("Car").ToList())
            {
                element.Name = "car";

                var engineElement = element.Element("engine");
                var modelElement = engineElement.Element("model");

                if (engineElement != null && modelElement != null)
                {
                    engineElement.SetAttributeValue("model", modelElement.Value);
                    modelElement.Remove();
                }
            }


            xmlDocument.Save(fileName);

            Console.WriteLine("Serializacja do XML zakończona pomyślnie.");
        }


        static List<Car> DeserializeFromXml(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            using (TextReader reader = new StreamReader(fileName))
            {
                return (List<Car>)serializer.Deserialize(reader);
            }
        }

        static void GenerateXhtmlDocument(List<Car> cars, string fileName)
        {
            XElement table = new XElement("table");
            foreach (var car in cars)
            {
                XElement row = new XElement("tr",
                    new XElement("td", car.Model),
                    new XElement("td", car.CarEngine.Displacement),
                    new XElement("td", car.CarEngine.HorsePower),
                    new XElement("td", car.Year)
                );
                table.Add(row);
            }

            XDocument xhtmlDocument = new XDocument(
                new XElement("html",
                    new XElement("head",
                        new XElement("title", "Cars Table")
                    ),
                    new XElement("body",
                        table
                    )
                )
            );

            xhtmlDocument.Save(fileName);
            Console.WriteLine($"Dokument XHTML został wygenerowany: {fileName}");
        }
    }
}
