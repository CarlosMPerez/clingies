namespace Clingies.App.Windows;

public partial class BodyControl : UserControl
{
    public event EventHandler<string>? ContentChanged;

    public string Content
    {
        get => ContentBox.Text ?? "";
        set => ContentBox.Text = value;
    }

    public ClingyBodyControl()
    {
        InitializeComponent();

        ContentBox.TextChanged += (_, _) =>
        {
            ContentChanged?.Invoke(this, ContentBox.Text ?? "");
        };
    }
}