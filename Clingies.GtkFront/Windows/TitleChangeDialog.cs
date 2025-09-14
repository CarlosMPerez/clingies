using System;
using Gtk;
using Gdk;
using Clingies.GtkFront.Services;

namespace Clingies.GtkFront.Windows
{
    /// <summary>
    /// Simple modal dialog to capture a new Clingy title.
    /// Returns null if cancelled/closed, or the entered title on OK.
    /// </summary>
    public sealed class TitleChangeDialog : Dialog
    {
        private readonly Entry _entry;

        private TitleChangeDialog(Gtk.Window? parent, string? initialTitle)
            : base("Title change", parent, DialogFlags.Modal)
        {
            // Window chrome/behavior
            TypeHint = WindowTypeHint.Dialog;     // dialog semantics (WMs usually show only Close)
            Modal = true;                         // block interaction with parent
            if (parent is not null) TransientFor = parent;                // stack above parent
            SkipTaskbarHint = true;               // do not show in taskbar
            KeepAbove = true;                     // ensure on top when opened
            Resizable = false;                    // fixed size
            SetDefaultSize(AppConstants.Dimensions.DefaultDialogWidth, AppConstants.Dimensions.DefaultDialogHeight);
            SetPosition(parent is not null ? WindowPosition.CenterOnParent : WindowPosition.Center);

            // Content area
            var content = (Box)ContentArea;
            content.BorderWidth = 12;
            content.Spacing = 4;

            var row = new Box(Orientation.Horizontal, 8);

            var label = new Label("Set Clingy title:");

            _entry = new Entry
            {
                Text = initialTitle ?? string.Empty,
                ActivatesDefault = true, // Pressing Enter triggers default button (OK)
                //InputPurpose = InputPurpose.FreeForm
            };
            GtkTweaks.MakeEntryCompact(_entry, pxHeight: 26, fontPt: 10);
            // optional: keep it visually tidy
            _entry.HasFrame = true;             // default, but good to be explicit
            _entry.Hexpand = true;
            _entry.Valign = Align.Center;
            _entry.ActivatesDefault = true;

            row.PackStart(label, false, false, 0);
            row.PackStart(_entry, true, true, 0);

            content.PackStart(row, true, true, 0);

            // Buttons (Cancel / OK). OK is default.
            AddButton("Cancel", ResponseType.Cancel);
            var okBtn = (Button)AddButton("OK", ResponseType.Ok);
            DefaultResponse = ResponseType.Ok;
            okBtn.CanDefault = true;
            okBtn.GrabDefault(); // make Enter hit OK

            // Closing the window via the title-bar close button = Cancel
            DeleteEvent += (_, e) =>
            {
                Respond(ResponseType.Cancel);
                e.RetVal = true; // we've handled it
            };

            ShowAll();
        }

        /// <summary>
        /// Shows the dialog modally and returns the result.
        /// Returns null if the user cancels/closes, otherwise the entered title.
        /// </summary>
        public static string? Show(Gtk.Window? parent, string? initialTitle = null)
        {
            using var dlg = new TitleChangeDialog(parent, initialTitle);

            // Run() blocks until a response (OK/Cancel/close)
            var resp = (ResponseType)dlg.Run();
            try
            {
                if (resp == ResponseType.Ok)
                {
                    // Return any text (allow empty string if the user wants no title)
                    return dlg._entry.Text;
                }
                return null; // cancelled or closed
            }
            finally
            {
                dlg.Hide();
            }
        }
    }
}
