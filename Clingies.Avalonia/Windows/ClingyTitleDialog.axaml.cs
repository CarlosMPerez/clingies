using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Clingies.Avalonia.Windows;

public partial class ClingyTitleDialog : Window
{
    public string TitleText => string.IsNullOrEmpty(TitleInput.Text) ? "" : TitleInput.Text;

    public ClingyTitleDialog()
    {
        InitializeComponent();
    }

    public ClingyTitleDialog(string initialTitle = "")
    {
        InitializeComponent();

        TitleInput.Text = initialTitle;
        TitleInput.AttachedToVisualTree += (_, _) =>
        {
            TitleInput.Focus();
            TitleInput.SelectionStart = 0;
            TitleInput.SelectionEnd = TitleInput.Text?.Length ?? 0;
        };

        OkButton.Click += (_, _) => Close(TitleInput.Text);
        CancelButton.Click += (_, _) => Close(null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // ENTER -> OK
        if (e.Key == Key.Return)
        {
            Close(TitleInput.Text);
        }
    }     
}