using System.Windows;
using System.Windows.Controls;

public static class ScrollViewerBehavior
{
    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBehavior),
            new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

    public static void SetHorizontalOffset(ScrollViewer viewer, double offset) =>
        viewer.SetValue(HorizontalOffsetProperty, offset);

    public static double GetHorizontalOffset(ScrollViewer viewer) =>
        (double)viewer.GetValue(HorizontalOffsetProperty);

    private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
            scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
    }

    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    public static void SetVerticalOffset(ScrollViewer viewer, double offset) =>
        viewer.SetValue(VerticalOffsetProperty, offset);

    public static double GetVerticalOffset(ScrollViewer viewer) =>
        (double)viewer.GetValue(VerticalOffsetProperty);

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
    }
}