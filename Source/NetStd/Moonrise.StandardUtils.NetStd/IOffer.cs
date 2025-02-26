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

namespace Moonrise.Utils.Standard
{
    /// <summary>
    ///     An interface that implements <see cref="IOffer" /> is an interface that has been declared to show what public
    ///     properties/methods are offered out by a class. This also then means that that class can be substituted - typically
    ///     for tests. Now there's nothing really new about this except the reasoning of why an interface has been defined. The
    ///     standard concept is that an interface is defined to say what is REQUIRED and tends to get passed in to the object
    ///     that REQUIRES them. An <see cref="IOffer" /> interface is what is OFFERED and is passed out, or rather actually
    ///     defined in the same namespace and then actually implemented by, the class that OFFERS itself out.
    ///     <para>
    ///         An <see cref="IOffer" /> interface MIGHT also make use of definitions, typically enums, that are found - now
    ///         wait for it,
    ///         this does conceptually hang together - inside the class that offers that interface, i.e. actually implements
    ///         it! So how does this hang together? An interface is supposed to be abstracted and separate from the
    ///         implementation
    ///         but you're reading that it actually DEPENDS on it's implementation? Yes you are my friend. THAT'S THE POINT, an
    ///         <see cref="IOffer" /> is OFFERED BY the implementation. The advantage for you, dear reader, is that by doing so
    ///         it means you can actually substitute it if you need to - essentially for testing!
    ///     </para>
    ///     An <see cref="IOffer" /> has no definition BEYOND indicating 'why' the interface has been offered.
    /// </summary>
    public interface IOffer { }
}