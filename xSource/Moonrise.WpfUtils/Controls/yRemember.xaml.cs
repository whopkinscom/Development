using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Moonrise.Utils.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for Remember.xaml
    /// </summary>
    public partial class Remember : UserControl
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.RegisterAttached("Data",
                                                                                                     typeof(bool),
                                                                                                     typeof(Remember),
                                                                                                     new PropertyMetadata(false, DataChangedCallback));

        private static readonly List<DependencyObject> monitoredDependencyObjects = new List<DependencyObject>();

        public static readonly DependencyProperty PropertyProperty = DependencyProperty.RegisterAttached("Property",
                                                                                                         typeof(string),
                                                                                                         typeof(Remember),
                                                                                                         new PropertyMetadata(string.Empty,
                                                                                                                              PropertyChangedCallback));

        public Remember()
        {
            InitializeComponent();
        }

        public static bool GetData(DependencyObject obj)
        {
            return (bool)obj.GetValue(DataProperty);
        }

        public static string GetProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyProperty);
        }

        public static void SetData(DependencyObject obj, bool value)
        {
            obj.SetValue(DataProperty, value);
        }

        public static void SetProperty(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyProperty, value);
        }

        private static void DataChangedCallback(DependencyObject dependencyObject,
                                                DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // Add the depObj to the list that we monitor
            monitoredDependencyObjects.Add(dependencyObject);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
        }
    }
}
