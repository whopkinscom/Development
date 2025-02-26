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

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     An attribute to indicate that a property will be evaluated as part of a <see cref="ValidateChildAttribute" />.
    ///     <para>
    ///         SOME validation implementations may make use of this to decide not to validate this "child" data, but only do
    ///         so in the context of
    ///         validating the "parent"!
    ///     </para>
    ///     <para>
    ///         You would typically use this where you want to use a composite view model with re-useable class data contained
    ///         in it that must be validated but use a specific message for failures of individual instances.
    ///     </para>
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidatedByParentAttribute : Attribute
    {
        /// <summary>
        ///     The error message. The use of this will be determined by
        ///     <seealso cref="ValidateChildAttribute.AppendChildMessage" />.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Indicates the name of the member to "highlight" if validation of the attributed member fails. Usually this will be
        ///     used where the attributed member does not have a visible element to highlight but another member does.
        /// </summary>
        public string MemberToHighlight { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatedByParentAttribute" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ValidatedByParentAttribute(string errorMessage) => ErrorMessage = errorMessage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatedByParentAttribute" /> class.
        /// </summary>
        public ValidatedByParentAttribute() { }
    }
}