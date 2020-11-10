#region Apache-v2.0

//    Copyright 2017 Will Hopkins - Moonrise Media Ltd.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Moonrise.Utils.Standard.Config;
using Moonrise.Utils.Wpf.Extensions;

namespace Moonrise.Utils.Wpf.Controls
{
    /// <summary>
    ///     Allows logical child controls to remember the value of a property
    /// </summary>
    public class Remember : ContentControl
    {
        /// <summary>
        ///     The data we hold for each child that has made use of the Remember controls attached properties
        /// </summary>
        protected class RememberedControlData
        {
            /// <summary>
            /// Metadata for the rememberd control's property
            /// </summary>
            public class PropertyData
            {
                /// <summary>
                ///     Information about the control's property being remembered
                /// </summary>
                public PropertyInfo ControlPropertyInfo { get; set; }

                /// <summary>
                ///     Cached information about the data context property
                /// </summary>
                public PropertyInfo DataContextPropertyInfo { get; set; }

                /// <summary>
                ///     The name property being remembered - also used when saving/restoring settings as well as finding the linked data
                ///     context value
                /// </summary>
                public string Property { get; set; }

                /// <summary>
                ///     Indicates if the property value should only be restored if there is an associated DataContext whose member the
                ///     property is bound to is null.
                /// </summary>
                public bool RestoreOnNullDC { get; set; } = true;

                /// <summary>
                ///     The name of the settings item - made up of <see cref="FrameworkElement.Name" /> + <see cref="ControlName" /> +
                ///     <see cref="Property" />.
                /// </summary>
                public string SettingsName { get; set; }
            }

            /// <summary>
            ///     The name of the control - used when saving/restoring settings.
            /// </summary>
            public string ControlName { get; set; }

            /// <summary>
            ///     A control can have a number of properties saved - likely only ever 1
            /// </summary>
            public Dictionary<string, PropertyData> Properties { get; set; } = new Dictionary<string, PropertyData>();
        }

        /// <summary>
        ///     The SaveOnCommand dependencyproperty
        /// </summary>
        public static readonly DependencyProperty SaveOnCommandProperty = DependencyProperty.Register(
            "SaveOnCommand",
            typeof(ICommand),
            typeof(Remember),
            new PropertyMetadata(null));

        /// <summary>
        ///     The Property attached property
        /// </summary>
        public static readonly DependencyProperty PropertyProperty = DependencyProperty.RegisterAttached("Property",
                                                                                                         typeof(string),
                                                                                                         typeof(Remember),
                                                                                                         new PropertyMetadata(string.Empty,
                                                                                                                              PropertyChangedCallback))
            ;

        /// <summary>
        ///     The RestoreOnNullDC attached property
        /// </summary>
        public static readonly DependencyProperty RestoreOnNullDCProperty = DependencyProperty.RegisterAttached("RestoreOnNullDC",
                                                                                                                typeof(bool),
                                                                                                                typeof(Remember),
                                                                                                                new PropertyMetadata(false,
                                                                                                                                     RestoreOnNullDCChangedCallback))
            ;

        /// <summary>
        ///     The list of controls that wish to be remembered by an instance of the Remember control
        /// </summary>
        private readonly Dictionary<DependencyObject, RememberedControlData> _rememberedControls =
            new Dictionary<DependencyObject, RememberedControlData>();

        private bool _restoreOnStartup;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Remember" /> class.
        /// </summary>
        public Remember()
        {
            RestoreOnStartup = true;
        }

        /// <summary>
        /// Not sure.
        /// </summary>
        public ICommand RelayTheCommand { get; set; }

        /// <summary>
        ///     Indicates if the Remember control should only restore a property if it's associated DataContext field is null
        /// </summary>
        public bool RestoreOnNullDC { get; set; } = true;

        /// <summary>
        ///     Indicates if the Remember control should restore its remembered properties on startup
        /// </summary>
        public bool RestoreOnStartup
        {
            get
            {
                return _restoreOnStartup;
            }
            set
            {
                _restoreOnStartup = value;

                if (_restoreOnStartup)
                {
                    Loaded += Remember_Loaded;
                }
            }
        }

        /// <summary>
        ///     Indicates if the Remember control should save its remembered properties on close
        /// </summary>
        public bool SaveOnClose { get; set; }

        /// <summary>
        ///     A command that when intercepted will cause the remembered fields to be rememeberd.
        /// </summary>
        public ICommand SaveOnCommand
        {
            get
            {
                return (ICommand)GetValue(SaveOnCommandProperty);
            }
            set
            {
                SetValue(SaveOnCommandProperty, value);
            }
        }

        /// <summary>
        ///     Indicates if the Remember control should save its remembered properties on losing focus of the particular control
        /// </summary>
        public bool SaveOnLostFocus { get; set; }

        /// <summary>
        ///     Gets the Property attached property value.
        /// </summary>
        /// <param name="obj">The object that is using the attached property</param>
        /// <returns>See Summary!</returns>
        public static string GetProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyProperty);
        }

        /// <summary>
        ///     Gets the RestoreOnNullDC attached property value.
        /// </summary>
        /// <param name="obj">The object that is using the attached property</param>
        /// <returns>See Summary!</returns>
        public static string GetRestoreOnNullDC(DependencyObject obj)
        {
            return (string)obj.GetValue(RestoreOnNullDCProperty);
        }

        /// <summary>
        ///     Sets the Property attached property value.
        /// </summary>
        /// <param name="obj">The object that is using the attached property</param>
        /// <param name="value">The value being set.</param>
        public static void SetProperty(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyProperty, value);
        }

        /// <summary>
        ///     Sets the RestoreOnNullDC attached property value.
        /// </summary>
        /// <param name="obj">The object that is using the attached property</param>
        /// <param name="value">The value being set.</param>
        public static void SetRestoreOnNullDC(DependencyObject obj, string value)
        {
            obj.SetValue(RestoreOnNullDCProperty, value);
        }

        /// <summary>
        ///     Indicates when the value of the Property attached property is changed
        /// </summary>
        /// <param name="dependencyObject">The dependency object the property is attached to.</param>
        /// <param name="dependencyPropertyChangedEventArgs">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance
        ///     containing the event data.
        /// </param>
        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            FrameworkElement parent = (FrameworkElement)dependencyObject;

            while (parent.GetType() != typeof(Remember))
            {
                parent = (FrameworkElement)parent.Parent;
            }

            Remember remember = (Remember)parent;
            remember.PropertyChanged(dependencyObject, dependencyPropertyChangedEventArgs);
        }

        /// <summary>
        ///     Indicates when the value of the RestoreOnNullDC attached property is changed
        /// </summary>
        /// <param name="dependencyObject">The dependency object the property is attached to.</param>
        /// <param name="dependencyPropertyChangedEventArgs">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance
        ///     containing the event data.
        /// </param>
        private static void RestoreOnNullDCChangedCallback(DependencyObject dependencyObject,
                                                           DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            FrameworkElement parent = (FrameworkElement)dependencyObject;

            while (parent.GetType() != typeof(Remember))
            {
                parent = (FrameworkElement)parent.Parent;
            }

            Remember remember = (Remember)parent;
            remember.RestoreOnNullDCChanged(dependencyObject, dependencyPropertyChangedEventArgs);
        }

        private void Execute(object o)
        {
            StoreProperties();

            RelayTheCommand.Execute(o);
        }

        /// <summary>
        ///     Obtains the remembered data, either by finding it in the dictionaries or creating a new one and adding it to the
        ///     appropriate dictionary.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///     <see cref="RememberedControlData" />
        /// </returns>
        private RememberedControlData.PropertyData ObtainRememberedData(Control control, string propertyName)
        {
            RememberedControlData.PropertyData retVal;
            RememberedControlData controlData;

            if (_rememberedControls.TryGetValue(control, out controlData))
            {
                if (!controlData.Properties.TryGetValue(propertyName, out retVal))
                {
                    retVal = new RememberedControlData.PropertyData();
                    retVal.Property = propertyName;
                    retVal.SettingsName = $"{Name}_{control.Name}_{propertyName}";
                    controlData.Properties[propertyName] = retVal;
                }
            }
            else
            {
                controlData = new RememberedControlData();
                controlData.ControlName = control.Name;
                retVal = new RememberedControlData.PropertyData();
                retVal.Property = propertyName;
                retVal.SettingsName = $"{Name}_{control.Name}_{propertyName}";
                controlData.Properties[propertyName] = retVal;
                _rememberedControls[control] = controlData;
            }

            return retVal;
        }

        /// <summary>
        ///     Handles the LostFocus event of a remembered control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            StoreProperties(sender);
        }

        /// <summary>
        ///     Instance handler for the value of the Property attached property being changed
        /// </summary>
        /// <param name="dependencyObject">The dependency object the property is attached to.</param>
        /// <param name="dependencyPropertyChangedEventArgs">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance
        ///     containing the event data.
        /// </param>
        private void PropertyChanged(DependencyObject dependencyObject,
                                     DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Control control = (Control)dependencyObject;
            string[] propertyNames = ((string)dependencyPropertyChangedEventArgs.NewValue).Split(new[]
                                                                                                 {
                                                                                                     ','
                                                                                                 },
                                                                                                 StringSplitOptions.RemoveEmptyEntries);

            foreach (string propertyName in propertyNames)
            {
                string propName = propertyName.Trim();
                RememberedControlData.PropertyData data = ObtainRememberedData(control, propName);
                data.ControlPropertyInfo = dependencyObject.GetType().GetProperty(data.Property);
            }

            if (SaveOnLostFocus)
            {
                control.LostFocus += OnLostFocus;
            }

            if (SaveOnClose)
            {
                // We only want one event response!
                Unloaded -= Remember_Unloaded;
                Unloaded += Remember_Unloaded;
            }

            if (SaveOnCommand != null)
            {
                CommandBinding cb = new CommandBinding(SaveOnCommand);
            }
        }

        /// <summary>
        ///     Handles the Loaded event of the Remember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Remember_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreRememberedProperties();
            if (SaveOnCommand != null) { }
        }

        /// <summary>
        ///     Handles the Unloaded event of a remembered control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Remember_Unloaded(object sender, RoutedEventArgs e)
        {
            StoreProperties();
        }

        /// <summary>
        ///     Instance handler for the value of the RestoreOnNullDC attached property being changed
        /// </summary>
        /// <param name="dependencyObject">The dependency object the property is attached to.</param>
        /// <param name="dependencyPropertyChangedEventArgs">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance
        ///     containing the event data.
        /// </param>
        private void RestoreOnNullDCChanged(DependencyObject dependencyObject,
                                            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            string propertyName = (string)dependencyPropertyChangedEventArgs.NewValue;
            Control control = (Control)dependencyObject;
            RememberedControlData.PropertyData data = ObtainRememberedData(control, propertyName);
            data.RestoreOnNullDC = (bool)dependencyPropertyChangedEventArgs.NewValue;
        }

        /// <summary>
        ///     Restores the remembered properties for all controls.
        /// </summary>
        private void RestoreRememberedProperties()
        {
            // Loop through every control
            foreach (KeyValuePair<DependencyObject, RememberedControlData> memory in _rememberedControls)
            {
                Control control = (Control)memory.Key;
                RestoreRememberedProperties(control);
            }
        }

        /// <summary>
        /// Restores the remembered properties for a specific control.
        /// </summary>
        /// <param name="control">The control.</param>
        private void RestoreRememberedProperties(Control control)
        {
            RememberedControlData controlData = _rememberedControls[control];

            // And every property in every control
            foreach (KeyValuePair<string, RememberedControlData.PropertyData> element in controlData.Properties)
            {
                RememberedControlData.PropertyData property = element.Value;
                bool restore = true;

                if (RestoreOnNullDC && property.RestoreOnNullDC)
                {
                    // We need to check if the DC binding for this property is null. If so, we restore, otherwise we leave it as is
                    if (property.DataContextPropertyInfo == null)
                    {
                        DependencyProperty dp = control.GetDependencyProperty(property.Property);

                        if (dp != null)
                        {
                            BindingExpression be = control.GetBindingExpression(dp);

                            if (be != null)
                            {
                                string dcMemberName = be.ResolvedSourcePropertyName;

                                if (!string.IsNullOrEmpty(dcMemberName))
                                {
                                    object dc = be.DataItem;
                                    property.DataContextPropertyInfo = dc.GetType().GetProperty(dcMemberName);
                                }
                            }
                        }
                    }

                    if (property.DataContextPropertyInfo == null)
                    {
                        restore = false;
                    }
                    else
                    {
                        object dcPropertyValue = property.DataContextPropertyInfo.GetValue(control.DataContext);

                        // Finally we check if the actual bound to element of the DataContext is null
                        restore = dcPropertyValue == null;
                    }
                }

                if (restore)
                {
                    string value = null;
                    Settings.User.Read(property.SettingsName, ref value, false);
                    property.ControlPropertyInfo.SetValue(control, value);
                }
            }
        }

        /// <summary>
        ///     Stores all of the properties for all remembered controls
        /// </summary>
        private void StoreProperties()
        {
            foreach (KeyValuePair<DependencyObject, RememberedControlData> memory in _rememberedControls)
            {
                StoreProperties(memory.Key);
            }
        }

        /// <summary>
        ///     Stores all of the properties for a particular control.
        /// </summary>
        /// <param name="control">The control to remember the properties for.</param>
        private void StoreProperties(object control)
        {
            RememberedControlData data = _rememberedControls[(DependencyObject)control];

            foreach (KeyValuePair<string, RememberedControlData.PropertyData> element in data.Properties)
            {
                RememberedControlData.PropertyData rememberedProperty = element.Value;

                if (rememberedProperty.ControlPropertyInfo != null)
                {
                    object propertyValue = rememberedProperty.ControlPropertyInfo.GetValue(control);
                    Settings.User.Write(rememberedProperty.SettingsName, propertyValue);
                }
            }
        }
    }
}
