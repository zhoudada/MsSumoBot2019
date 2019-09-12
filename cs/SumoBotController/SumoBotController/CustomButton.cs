using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SumoBotController
{
    public class CustomButton : Button
    {
        public event PointerEventHandler Pressed;
        public event PointerEventHandler Released;

        protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Pressed?.Invoke(this, e);
            e.Handled = true;
        }

        protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Released?.Invoke(this, e);
            e.Handled = true;
        }
    }
}
