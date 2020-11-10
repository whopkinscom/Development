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
using System.ComponentModel;
using System.Windows;

namespace Moonrise.Utils.Wpf.Extensions
{
    /// <summary>
    /// Extensions for <see cref="DependencyObject"/>
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Gets the named <see cref="DependencyProperty"/> on a <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="depObj">The dep object being extended.</param>
        /// <param name="propertyName">Name of the dependency property.</param>
        /// <returns>The <see cref="DependencyProperty"/> or null if not found</returns>
        public static DependencyProperty GetDependencyProperty(this DependencyObject depObj, string propertyName)
        {
            foreach (PropertyDescriptor descr in TypeDescriptor.GetProperties(depObj,
                                                                              new Attribute[]
                                                                              {
                                                                                  new PropertyFilterAttribute(PropertyFilterOptions.All)
                                                                              })
            )
            {
                DependencyPropertyDescriptor dpDescr = DependencyPropertyDescriptor.FromProperty(descr);

                if ((dpDescr != null) && (dpDescr.Name == propertyName))
                {
                    return dpDescr.DependencyProperty;
                }
            }

            return null;
        }
    }
}
