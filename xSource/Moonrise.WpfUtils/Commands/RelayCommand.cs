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
using System.Diagnostics;
using System.Windows.Input;

namespace Moonrise.Utils.Wpf.Commands
{
    /// <summary>
    ///     Relay command represents a command with no parameters and is simply an instruction.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        public RelayCommand(Action<object> execute) :
            this(execute, null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate that determines if the command can execute.</param>
        /// <exception cref="System.ArgumentNullException">execute</exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    ///     Defines a generic command interface. that can appear in a required data interface as the command required.
    /// </summary>
    /// <typeparam name="T">The type of data that will be passed to the command</typeparam>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public interface ICommand<T> : ICommand { }

    /// <summary>
    ///     Generic relay command implementation to allow commands to be passed in a datacontext.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand<T> : ICommand<T>
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        public RelayCommand(Action<T> execute) :
            this(execute, null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate that determines if the command can execute.</param>
        /// <exception cref="System.ArgumentNullException">execute</exception>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
