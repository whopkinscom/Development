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
using System.Windows;
using System.Windows.Controls;

namespace Moonrise.Utils.Wpf.Controls
{
    /// <summary>
    ///     A stack panel that better aligns its contents. This was ripped straight from
    ///     http://stackoverflow.com/questions/1983134/how-can-i-make-elements-arranged-in-a-horizontal-stackpanel-share-a-common-basel
    /// </summary>
    /// <seealso cref="System.Windows.Controls.StackPanel" />
    public class AlignedStackPanel : StackPanel
    {
        /// <summary>
        ///     Determines if the text should be aligned at the top or the bottom of the panel.
        /// </summary>
        public bool AlignTop { get; set; }

        /// <summary>
        ///     Arranges the content of a <see cref="T:System.Windows.Controls.StackPanel" /> element.
        /// </summary>
        /// <param name="arrangeSize">
        ///     The <see cref="T:System.Windows.Size" /> that this element should use to arrange its child
        ///     elements.
        /// </param>
        /// <returns>
        ///     The <see cref="T:System.Windows.Size" /> that represents the arranged size of this
        ///     <see cref="T:System.Windows.Controls.StackPanel" /> element and its child elements.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection children = Children;
            bool fHorizontal = Orientation == Orientation.Horizontal;
            Rect rcChild = new Rect(arrangeSize);
            double previousChildSize = 0.0;

            for (int i = 0,
                     count = children.Count;
                 i < count;
                 ++i)
            {
                UIElement child = children[i];

                if (child == null)
                {
                    continue;
                }

                if (fHorizontal)
                {
                    double offset = GetStackElementOffset(child);

                    if (AlignTop)
                    {
                        rcChild.Y = offset;
                    }

                    rcChild.X += previousChildSize;
                    previousChildSize = child.DesiredSize.Width;
                    rcChild.Width = previousChildSize;
                    rcChild.Height = Math.Max(arrangeSize.Height - offset, child.DesiredSize.Height);
                }
                else
                {
                    rcChild.Y += previousChildSize;
                    previousChildSize = child.DesiredSize.Height;
                    rcChild.Height = previousChildSize;
                    rcChild.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);
                }

                child.Arrange(rcChild);
            }

            return arrangeSize;
        }

        /// <summary>
        /// Gets the stack element offset which differs for different elements. Override this to support further elements than;
        /// <see cref="TextBox"/>
        /// <see cref="TextBlock"/>
        /// <see cref="Label"/>
        /// <see cref="ComboBox"/>
        /// </summary>
        /// <param name="stackElement">The stack element.</param>
        /// <returns>The appropriate offset</returns>
        protected virtual double GetStackElementOffset(UIElement stackElement)
        {
            if (stackElement is TextBlock)
            {
                return 5;
            }

            if (stackElement is Label)
            {
                return 0;
            }

            if (stackElement is TextBox)
            {
                return 2;
            }

            if (stackElement is ComboBox)
            {
                return 2;
            }

            return 0;
        }

        /// <summary>
        ///     Measures the child elements of a <see cref="T:System.Windows.Controls.StackPanel" /> in anticipation of arranging
        ///     them during the <see cref="M:System.Windows.Controls.StackPanel.ArrangeOverride(System.Windows.Size)" /> pass.
        /// </summary>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
        /// <returns>
        ///     The <see cref="T:System.Windows.Size" /> that represents the desired size of the element.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size stackDesiredSize = new Size();

            UIElementCollection children = InternalChildren;
            Size layoutSlotSize = constraint;
            bool fHorizontal = Orientation == Orientation.Horizontal;

            if (fHorizontal)
            {
                layoutSlotSize.Width = double.PositiveInfinity;
            }
            else
            {
                layoutSlotSize.Height = double.PositiveInfinity;
            }

            for (int i = 0,
                     count = children.Count;
                 i < count;
                 ++i)
            {
                // Get next child.
                UIElement child = children[i];

                if (child == null)
                {
                    continue;
                }

                // Accumulate child size.
                if (fHorizontal)
                {
                    // Find the offset needed to line up the text and give the child a little less room.
                    double offset = GetStackElementOffset(child);
                    child.Measure(new Size(double.PositiveInfinity, constraint.Height - offset));
                    Size childDesiredSize = child.DesiredSize;

                    stackDesiredSize.Width += childDesiredSize.Width;
                    stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, childDesiredSize.Height + GetStackElementOffset(child));
                }
                else
                {
                    child.Measure(layoutSlotSize);
                    Size childDesiredSize = child.DesiredSize;

                    stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                    stackDesiredSize.Height += childDesiredSize.Height;
                }
            }

            return stackDesiredSize;
        }
    }
}
