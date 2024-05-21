using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace lab4_tym_razem_poprawne
{
    public partial class MainWindow : Window
    {

        // Klasa Engine z implementacją IComparable
        public class Engine : IComparable<Engine>
        {
            public double Displacement { get; set; }
            public double HorsePower { get; set; }
            public string Model { get; set; }

            public Engine() { }

            public Engine(double displacement, double horsePower, string model)
            {
                Displacement = displacement;
                HorsePower = horsePower;
                Model = model;
            }

            public int CompareTo(Engine other)
            {
                return HorsePower.CompareTo(other.HorsePower);
            }
        }

        // Klasa Car
        public class Car
        {
            public string Model { get; set; }
            public int Year { get; set; }
            public Engine CarEngine { get; set; }

            public Car() { }

            public Car(string model, Engine engine, int year)
            {
                Model = model;
                CarEngine = engine;
                Year = year;
            }
        }

        // SortableBindingList<T>
        public class SortableBindingList<T> : BindingList<T>
        {
            private bool isSorted;
            private ListSortDirection sortDirection;
            private PropertyDescriptor sortProperty;

            // Dodaj konstruktor przyjmujący listę obiektów
            public SortableBindingList(IEnumerable<T> collection) : base(collection.ToList())
            {
            }

            protected override bool SupportsSortingCore => true;

            protected override ListSortDirection SortDirectionCore => sortDirection;

            protected override PropertyDescriptor SortPropertyCore => sortProperty;

            protected override bool IsSortedCore => isSorted;

            protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
            {
                if (typeof(IComparable).IsAssignableFrom(prop.PropertyType))
                {
                    List<T> items = this.Items as List<T>;
                    if (items != null)
                    {
                        items.Sort((x, y) =>
                        {
                            IComparable xValue = (IComparable)prop.GetValue(x);
                            IComparable yValue = (IComparable)prop.GetValue(y);

                            return direction == ListSortDirection.Ascending ? xValue.CompareTo(yValue) : yValue.CompareTo(xValue);
                        });

                        isSorted = true;
                        sortProperty = prop;
                        sortDirection = direction;
                    }
                }
                else
                {
                    isSorted = false;
                }

                this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }

            protected override void RemoveSortCore()
            {
                isSorted = false;
            }

            protected override bool SupportsSearchingCore => true;

            protected override int FindCore(PropertyDescriptor prop, object key)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (prop.GetValue(this[i]).Equals(key))
                        return i;
                }
                return -1;
            }
        }

        SortableBindingList<Car> myCarsBindingList;

        public MainWindow()
        {
            InitializeComponent();

            var myCars = new List<Car>
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
                // Dodaj inne samochody do kolekcji
            };

            myCarsBindingList = new SortableBindingList<Car>(myCars);

            dataGridView1.ItemsSource = myCarsBindingList;

            dataGridView1.AutoGeneratingColumn += (s, e) =>
            {
                if (e.PropertyType == typeof(Engine))
                {
                    e.Cancel = true;
                }
            };

            propertyComboBox.ItemsSource = typeof(Car).GetProperties().Select(p => p.Name);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

            string selectedProperty = propertyComboBox.SelectedItem?.ToString();
            string searchTerm = valueTextBox.Text;

            if (!string.IsNullOrEmpty(selectedProperty) && !string.IsNullOrEmpty(searchTerm))
            {
                PropertyInfo propertyInfo = typeof(Car).GetProperty(selectedProperty);
                if (propertyInfo != null)
                {
                    object convertedValue = Convert.ChangeType(searchTerm, propertyInfo.PropertyType);
                    int index = myCarsBindingList.IndexOf(myCarsBindingList.FirstOrDefault(car => propertyInfo.GetValue(car).Equals(convertedValue)));

                    if (index != -1)
                    {
                        dataGridView1.ScrollIntoView(dataGridView1.Items[index]);
                        dataGridView1.SelectedItem = myCarsBindingList[index];
                    }
                    else
                    {
                        MessageBox.Show("No matching car found.");
                    }
                }
            }
        }
    }
}
