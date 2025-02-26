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
using System.Runtime.CompilerServices;
using Moonrise.Logging.Util;

namespace Moonrise.Logging
{
    /// <summary>
    ///     Enables more granular logging control, by allowing (or not), logging associated with different
    ///     <see cref="LogTag" />s.
    ///     <para>Usage:</para>
    ///     <para>
    ///         Define in some functional group global class a LogTag instance with a "unique-ish" name. Pass this tag in those
    ///         specific logging calls.
    ///     </para>
    ///     <para>
    ///         In your logging initialisation phase, pass a list of strings with the tags you want allowed to be logged. All
    ///         non-tagged logs will still work, however any tagged logs not in that list will not be logged. The usual logging
    ///         level still applies though.
    ///     </para>
    ///     We use tag instances rather than simply strings, as they're easier to make consistent and not have misspled tag
    ///     names!
    /// </summary>
    public class LogTag
    {
        /// <summary>
        ///     A <see cref="LogTag" /> that is scoped within the thread.
        /// </summary>
        /// <seealso cref="LogTag" />
        public class Scoped : ScopedNestableThreadGlobalSingleton<LogTag>
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="Scoped" /> class.
            /// </summary>
            /// <param name="value">The value which will be the current NestedThreadGlobal value.</param>
            public Scoped(LogTag value)
                : base(value) { }
        }

        /// <summary>
        ///     Prevents infinte looping if a tag directly or indirectly is nested beneath itself.
        /// </summary>
        private bool beenChecked;

        /// <summary>
        ///     The log tag name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The parent tag indicating the tag is nested
        /// </summary>
        protected LogTag Parent { get; set; }

        /// <summary>
        ///     The scoped parent tag indicating the tag is nested in scope
        /// </summary>
        protected LogTag ScopeParent { get; set; }

        /// <summary>
        ///     The active log tags
        /// </summary>
        internal static List<string> ActiveLogTags { get; set; } = new List<string>();

        /// <summary>
        ///     The encountered log tags. We build up a "list" of log tags that have attempted to be used, regardless of whether
        ///     they're active or not.
        /// </summary>
        internal static List<LogTag> EncounteredLogTags { get; set; } = new List<LogTag>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogTag" /> class.
        /// </summary>
        /// <param name="name">The tag name.</param>
        public LogTag(string name)
        {
            Name = name;
            EncounteredLogTags.Add(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogTag" /> class. Use this version if you want to nest this tag under
        ///     another existing tag.
        ///     <para>
        ///         If a parent is active, all nested tags are also considered active.
        ///     </para>
        ///     <para>
        ///         NOTE: LogTags are clever enough not to loop forever when checking if parent tags are active but there's no
        ///         other checking for how you manage your nesting, so be sensible.
        ///     </para>
        /// </summary>
        /// <param name="name">The tag name.</param>
        /// <param name="parentLogTag">A parent log tag so that you can group and nest tags.</param>
        public LogTag(string name, LogTag parentLogTag)
        {
            Name = name;
            Parent = parentLogTag;
            EncounteredLogTags.Add(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogTag" /> class. Use this version if you want to nest other tags
        ///     underneath this tag.
        ///     <para></para>
        ///     <para>
        ///         If a parent is active, all nested tags are also considered active.
        ///     </para>
        ///     <para>
        ///         NOTE: LogTags are clever enough not to loop forever when checking if parent tags are active but there's no
        ///         other checking for how you manage your nesting, so be sensible.
        ///     </para>
        /// </summary>
        /// <param name="name">The tag name.</param>
        /// <param name="childLogTags">The child log tags, each of which will be updated to make this tag their parent.</param>
        /// <param name="parentLogTag">The parent log tag.</param>
        public LogTag(string name, IEnumerable<LogTag> childLogTags, LogTag parentLogTag = null)
        {
            Name = name;
            EncounteredLogTags.Add(this);

            foreach (LogTag childLogTag in childLogTags)
            {
                childLogTag.Parent = this;
            }

            Parent = parentLogTag;
        }

        /// <summary>
        ///     Activates the log tag by adding it to the list of those already active.
        /// </summary>
        /// <param name="tagName">Name of the tag - null not allowed!.</param>
        public static void ActivateLogTag(string tagName)
        {
            ActiveLogTags.Add(tagName);
        }

        /// <summary>
        ///     Activates the log tag by adding it to the list of those already active.
        /// </summary>
        /// <param name="tag">LogTag to activate</param>
        public static void ActivateLogTag(LogTag tag)
        {
            ActiveLogTags.Add(tag.Name);
        }

        /// <summary>
        ///     Activates the log tags by adding the supplied tags to those already active. If you want to reset the list, pass a
        ///     null so you can start again.
        /// </summary>
        /// <param name="tagNames">The tag names or null to reset the list.</param>
        public static void ActivateLogTags(IEnumerable<string> tagNames)
        {
            if (tagNames == null)
            {
                ActiveLogTags.Clear();
            }
            else
            {
                ActiveLogTags.AddRange(tagNames);
            }
        }

        /// <summary>
        ///     Deactivates the log tag by removing it from the list of those already active.
        /// </summary>
        /// <param name="tag">The tag to deactivate</param>
        public static void DeactivateLogTag(LogTag tag)
        {
            ActiveLogTags.Remove(tag.Name);
        }

        /// <summary>
        ///     Deactivates the log tag by removing it from the list of those already active.
        /// </summary>
        /// <param name="tagName">Name of the tag - null not allowed!.</param>
        public static void DeactivateLogTag(string tagName)
        {
            ActiveLogTags.Remove(tagName);
        }

        /// <summary>
        ///     Deactivates the log tags by removing the supplied tags from those already active.
        /// </summary>
        /// <param name="tagNames">The tag names</param>
        public static void DeactivateLogTags(IEnumerable<string> tagNames)
        {
            foreach (string tagName in tagNames)
            {
                ActiveLogTags.Remove(tagName);
            }
        }

        /// <summary>
        ///     Wraps the action in a Logger.Context and logs any exceptions before passing them on.
        ///     <para>
        ///         Usage: MyLogTag.Do(()=>{code;});
        ///     </para>
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="caller">The caller.</param>
        public void Do(Action action, [CallerMemberName] string caller = "")
        {
            try
            {
                using (Logger.Context(this, caller))
                {
                    action();
                }
            }
            catch (Exception excep)
            {
                Logger.Error(excep, $"Exception in {caller}(...)");
                throw;
            }
        }

        /// <summary>
        ///     Determines whether the specified log tag is either null or active.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the specified log tag is either null or active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsActive()
        {
            beenChecked = true;
            bool retVal = ActiveLogTags.Contains(Name);

            if (!retVal && Parent != null && !Parent.beenChecked)
            {
                retVal = Parent.IsActive();
            }

            if (!retVal && ScopeParent != null && !ScopeParent.beenChecked)
            {
                retVal = ScopeParent.IsActive();
            }

            beenChecked = false;
            return retVal;
        }

        /// <summary>
        ///     Creates a new tag that is scoped within the current thread. This results in all non tagged logs within this scope
        ///     being automatically tagged. This is a way of controlling ALL logging that might occur within scope without needing
        ///     to add tags to each statement.
        /// </summary>
        /// <returns></returns>
        public Scoped Scope()
        {
            LogTag currentScope = Scoped.CurrentValue;
            LogTag newScope = new LogTag(string.Empty, this);
            Scoped retVal = new Scoped(newScope);

            if (currentScope != null)
            {
                newScope.ScopeParent = currentScope;
            }

            return retVal;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;
    }
}