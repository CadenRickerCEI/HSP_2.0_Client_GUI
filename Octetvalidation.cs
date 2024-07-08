namespace HSPGUI.Resources
{
    /// <summary>
    /// The OctetValidationBehavior class is a behavior for .NET MAUI Entry controls
    /// that validates the text input to ensure it is a valid octet (0-255).
    /// </summary>
    public class OctetValidationBehavior : Behavior<Entry>
    {
        /// <summary>
        /// Attaches the behavior to the specified Entry control.
        /// </summary>
        /// <param name="entry">The Entry control to attach the behavior to.</param>
        protected override void OnAttachedTo(Entry entry)
        {
            entry!.TextChanged += OnEntryTextChanged!;
            base.OnAttachedTo(entry);
        }

        /// <summary>
        /// Detaches the behavior from the specified Entry control.
        /// </summary>
        /// <param name="entry">The Entry control to detach the behavior from.</param>
        protected override void OnDetachingFrom(Entry entry)
        {
            entry!.TextChanged -= OnEntryTextChanged!;
            base.OnDetachingFrom(entry);
        }

        /// <summary>
        /// Event handler for the TextChanged event of the Entry control.
        /// Validates the new text value to ensure it is a valid octet (0-255).
        /// Changes the text color to black if valid, or red if invalid.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="args">The event arguments.</param>
        void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (int.TryParse(args.NewTextValue, out int result))
            {
                ((Entry)sender).TextColor = (result >= 0 && result <= 255) ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
            }
            else
            {
                ((Entry)sender).TextColor = Color.FromArgb("#FF0000");
            }
        }
    }
}
