using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


namespace LR3
{
    class Program
    {
        public enum PlaneType
        {
            None,
            AirbusA310,
            AirbusA320,
            Boeing737,
            Boeing747
        }

        [XmlInclude(typeof(PessengerPlane))]
        [XmlInclude(typeof(CargoAircraft))]
        public abstract class Plane
        {

            private static readonly Dictionary<PlaneType, double> _emptyWeights =
                new Dictionary<PlaneType, double>
                {
                    [PlaneType.AirbusA310] = 82000,
                    [PlaneType.AirbusA320] = 36750,
                    [PlaneType.Boeing737] = 26400,
                    [PlaneType.Boeing747] = 186000
                };

            protected PlaneType _planeType;
            protected double _emptyWeight;

            protected Plane() { }

            protected Plane(PlaneType type, string number)
            {
                Type = type;
                number = number;
            }

            public PlaneType Type
            {
                get { return _planeType; }
                set
                {
                    _planeType = value;
                    _emptyWeight = _emptyWeights[_planeType];
                }
            }

            public string Number { get; set; }

            public abstract double TakeoffWeight { get; }
        }

        public class PessengerPlane : Plane
        {
            private const double K = 62;

            public PessengerPlane() { }

            public PessengerPlane(PlaneType type, string number, int count)
                : base(type, number)
            {
                Count = count;
            }


            public int Count { get; set; }

            public override double TakeoffWeight
            {
                get { return K * Count + _emptyWeight; }
            }
        }

        public class CargoAircraft : Plane
        {
            public CargoAircraft() { }

            public CargoAircraft(PlaneType type, string number, double weight)
                : base(type, number)
            {
                CargoWeight = weight;
            }


            public double CargoWeight { get; set; }

            public override double TakeoffWeight
            {
                get { return CargoWeight + _emptyWeight; }
            }
        }

        public class Airline
        {
            private readonly List<Plane> _planes = new List<Plane>();


            public double TotalWeight
            {
                get
                {
                    double weight = 0;

                    foreach (var plane in _planes)
                        weight += plane.TakeoffWeight;

                    return weight;
                }
            }

            public void Add(Plane plane)
            {
                if (plane == null || plane.Type == PlaneType.None || string.IsNullOrEmpty(plane.Number))
                {
                    throw new ArgumentException(nameof(plane));
                }

                _planes.Add(plane);
            }

            public IEnumerable<Plane> GetPlanes()
            {
                return _planes;
            }

            private class ByWeightComparer : IComparer<Plane>
            {
                public int Compare(Plane x, Plane y)
                {
                    return x.TakeoffWeight.CompareTo(y.TakeoffWeight);
                }
            }

            /// <summary>
            /// Сортировка по весу
            /// </summary>
            /// <param name="args"></param>
            public void SortByWeight()
            {
                _planes.Sort(new ByWeightComparer());
            }

            public void ToXml(string filename)
            {
                var serializer = new XmlSerializer(typeof(List<Plane>));

                using (var stream = File.OpenWrite(filename))
                {
                    serializer.Serialize(stream, _planes);
                    stream.Flush();
                }
            }

            public static Airline FromXml(string filename)
            {
                var airline = new Airline();
                var serializer = new XmlSerializer(typeof(List<Plane>));

                using (var stream = File.OpenRead(filename))
                {
                    var planes = serializer.Deserialize(stream) as IEnumerable<Plane>;
                    if (planes != null) airline._planes.AddRange(planes);
                }

                return airline;
            }

            public void ToJson(string filename)
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(_planes, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }));
            }

            public static Airline FromJson(string filename)
            {
                var airline = new Airline();

                var planes = JsonConvert.DeserializeObject<List<Plane>>(File.ReadAllText(filename), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                if (planes != null) airline._planes.AddRange(planes);
                return airline;
            }
        }






        static void Main(string[] args)
        {
            var myAirline = new Airline();
            var exit = false;
            Console.OutputEncoding = System.Text.Encoding.Unicode;


            while (exit == false)
            {
                Console.WriteLine("\nВыберите один из вариантов\n");
                Console.WriteLine("1 - Добавить самолет");
                Console.WriteLine("2 - Показать список");
                Console.WriteLine("3 - Сортировка по весу");
                Console.WriteLine("4 - Средний взлетный вес самолета");
                Console.WriteLine("5 - Выйти из программы");

                var input = Console.ReadKey();

                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("\nOption 1 selected");
                        break;
                    case ConsoleKey.D2:
                        Console.WriteLine("\nOption 2 selected");
                        break;
                    case ConsoleKey.D3:
                        Console.WriteLine("\nOption 3 selected");
                        break;
                    case ConsoleKey.D4:
                        Console.WriteLine("\nOption 4 selected");
                        break;
                    case ConsoleKey.D5:
                        Console.WriteLine("\nOption 5 selected");
                        Console.WriteLine("Program exiting...");
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("\nCommand not recognised.");
                        break;
                }
            }
        }
    }
}
