using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Moonrise.Utils.Wpf.Controls
{
    /// <summary>
    ///     Interaction logic for Remember.xaml
    /// </summary>
    public partial class Remember : ContentControl
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
