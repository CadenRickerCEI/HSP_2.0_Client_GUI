namespace HSPGUI.Resources
{
    public class OctetValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry!.TextChanged += OnEntryTextChanged!;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry!.TextChanged -= OnEntryTextChanged!;
            base.OnDetachingFrom(entry);
        }

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
