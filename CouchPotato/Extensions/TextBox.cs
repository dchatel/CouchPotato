using System.Windows;
using System.Windows.Input;

namespace CouchPotato.Extensions;

public static class TextBox
{
    public static readonly DependencyProperty IsInitiallyFocusedProperty =
      DependencyProperty.RegisterAttached(
        "IsInitiallyFocused",
        typeof(bool),
        typeof(System.Windows.Controls.TextBox),
        new FrameworkPropertyMetadata(false, OnIsInitiallyFocusedPropertyChanged));

    public static bool GetIsInitiallyFocused(UIElement element) =>
      (bool)element.GetValue(IsInitiallyFocusedProperty);

    public static void SetIsInitiallyFocused(UIElement element, bool value) =>
      element.SetValue(IsInitiallyFocusedProperty, value);

    private static void OnIsInitiallyFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element || !(bool)e.NewValue)
        {
            return;
        }

        element.Dispatcher.BeginInvoke(() => FocusManager.SetFocusedElement(element, element));
    }
}
