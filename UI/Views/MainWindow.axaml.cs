using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Dragon_Workspaces.UI.Views;

public partial class MainWindow : Window
{
    private readonly double configuredMinHeight;
    private readonly double configuredMaxHeight;

    public MainWindow()
    {
        InitializeComponent();
        configuredMinHeight = MinHeight;
        configuredMaxHeight = MaxHeight;
        Opened += (_, _) => ApplyFixedHeightDragWorkaround();
    }

    private void HeaderDragRegion_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        BeginMoveDrag(e);
    }

    private void ApplyFixedHeightDragWorkaround()
    {
        if (CanResize)
        {
            MinHeight = configuredMinHeight;
            MaxHeight = configuredMaxHeight;
            return;
        }

        if (double.IsNaN(Height) || Height <= 0)
        {
            return;
        }

        MinHeight = Height;
        MaxHeight = Height;
    }
}
