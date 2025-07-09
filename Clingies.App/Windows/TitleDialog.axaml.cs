using Avalonia.Controls;
using Avalonia.Input;

namespace Clingies.App.Windows;

public partial class TitleDialog : Window
{
    public string TitleText => string.IsNullOrEmpty(TitleInput.Text) ? "" : TitleInput.Text;

    public TitleDialog()
    {
        InitializeComponent();
    }

    public TitleDialog(string initialTitle = "")
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
