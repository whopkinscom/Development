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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Moonrise.Utils.Wpf.Notifications
{
    /// <summary>
    ///     Use this as base class for implementing notifying properties within a ViewModel!
    /// </summary>
    public class NotifyingData : INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Sets a field and sends a property changed notification if the value changed
        /// </summary>
        /// <typeparam name="T">implied type based on the parameters passed</typeparam>
        /// <param name="field">Backing field for the property</param>
        /// <param name="value">New value for the property</param>
        /// <param name="propertyName">This gets autofilled by the c# 5+ compiler</param>
        /// Here's a template for how to use this for an example boolean Edited property;
        /// 
        /// bool edited;
        /// public bool Edited
        /// {
        /// get { return edited; }
        /// set { SetField(ref edited, value); }
        /// }
        protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
