using System.Collections.Generic;
using System.Linq;
using Gtk;
using Gdk;
using Pango;

namespace Clingies.GtkFront.Windows
{
    public sealed class StyleManagerWindow : Dialog
    {
        // Styles list
        public TreeView StylesView { get; private set; }
        public ListStore StylesStore { get; private set; }

        // Editor controls
        public Entry EntryStyleName { get; private set; }
        public CheckButton ChkActive { get; private set; }
        public CheckButton ChkDefault { get; private set; }
        public ComboBoxText CmbFont { get; private set; }
        public SpinButton SpinFontSize { get; private set; }
        public ColorButton BtnFontColor { get; private set; }
        public ColorButton BtnBodyColor { get; private set; }
        public CheckButton CbBold { get; private set; }
        public CheckButton CbItalic { get; private set; }
        public CheckButton CbUnderline { get; private set; }
        public CheckButton CbStrike { get; private set; }

        // Actions
        public Button BtnDelete { get; private set; }
        public Button BtnSave { get; private set; }

        public StyleManagerWindow(Gtk.Window parent = null) : base("Style Manager", parent, DialogFlags.Modal)
        {
            // ---- dialog/window behavior ----
            DefaultWidth = 560;
            DefaultHeight = 420;
            BorderWidth = 8;

            // Treat as a dialog, no min/max, no taskbar entry, not resizable
            TypeHint = WindowTypeHint.Dialog;
            Resizable = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;
            // Keep dialog above parent (optional, common for dialogs)
            KeepAbove = true;

            if (parent != null)
            {
                TransientFor = parent;
                SetPosition(WindowPosition.CenterOnParent);
            }
            else
            {
                SetPosition(WindowPosition.Center);
            }

            BuildUi();
            WireSignals();
            PopulateFontFamilies();

            ShowAll();
        }

        void BuildUi()
        {
            var root = new Box(Orientation.Vertical, 8);

            // === Styles list ==========================================================
            var stylesFrame = new Frame { Label = "Styles" };
            var scStyles = new ScrolledWindow();
            StylesView = new TreeView { HeadersVisible = false };
            StylesStore = new ListStore(typeof(int), typeof(string));
            StylesView.Model = StylesStore;

            var col = new TreeViewColumn();
            var cell = new CellRendererText();
            col.PackStart(cell, true);
            col.AddAttribute(cell, "text", 1);
            StylesView.AppendColumn(col);
            StylesView.Selection.Mode = SelectionMode.Single;

            scStyles.Add(StylesView);
            stylesFrame.Add(scStyles);

            // === Editor ===============================================================
            var editorFrame = new Frame { Label = "Style" };
            var grid = new Grid { ColumnSpacing = 8, RowSpacing = 8, Margin = 8 };

            int r = 0;
            grid.Attach(new Label("Style Name") { Xalign = 0 }, 0, r, 1, 1);
            EntryStyleName = new Entry { Name = "style_name" };
            grid.Attach(EntryStyleName, 1, r, 2, 1);

            var flagsBox = new Box(Orientation.Horizontal, 8);
            ChkActive  = new CheckButton("Active");
            ChkDefault = new CheckButton("Default");
            flagsBox.PackStart(ChkActive, false, false, 0);
            flagsBox.PackStart(ChkDefault, false, false, 0);
            grid.Attach(flagsBox, 3, r, 1, 1);

            r++;
            grid.Attach(new Label("Font") { Xalign = 0 }, 0, r, 1, 1);
            CmbFont = new ComboBoxText { Name = "font_family" };
            grid.Attach(CmbFont, 1, r, 3, 1);

            r++;
            grid.Attach(new Label("Font Size") { Xalign = 0 }, 0, r, 1, 1);
            SpinFontSize = new SpinButton(new Adjustment(12, 6, 144, 1, 5, 0), 1, 0) { Name = "font_size" };
            grid.Attach(SpinFontSize, 1, r, 1, 1);

            grid.Attach(new Label("Font Color") { Xalign = 1 }, 2, r, 1, 1);
            BtnFontColor = new ColorButton { Name = "font_color" };
            grid.Attach(BtnFontColor, 3, r, 1, 1);

            r++;
            grid.Attach(new Label("Body Color") { Xalign = 1 }, 2, r, 1, 1);
            BtnBodyColor = new ColorButton { Name = "body_color" };
            grid.Attach(BtnBodyColor, 3, r, 1, 1);

            r++;
            grid.Attach(new Label("Font Decorations") { Xalign = 0 }, 0, r, 1, 1);
            var decoBox = new Box(Orientation.Horizontal, 12);
            CbBold      = new CheckButton("Bold");
            CbItalic    = new CheckButton("Italics");
            CbUnderline = new CheckButton("Underline");
            CbStrike    = new CheckButton("Strikethrough");
            decoBox.PackStart(CbBold, false, false, 0);
            decoBox.PackStart(CbItalic, false, false, 0);
            decoBox.PackStart(CbUnderline, false, false, 0);
            decoBox.PackStart(CbStrike, false, false, 0);
            grid.Attach(decoBox, 1, r, 3, 1);

            editorFrame.Add(grid);

            // === Actions ==============================================================
            var actions = new Box(Orientation.Horizontal, 8) { Halign = Align.End };
            BtnDelete = new Button("Delete");
            BtnDelete.StyleContext.AddClass("destructive-action");
            BtnSave = new Button("Save");
            BtnSave.StyleContext.AddClass("suggested-action");
            actions.PackEnd(BtnSave, false, false, 0);
            actions.PackEnd(BtnDelete, false, false, 0);

            // === Pack into dialog content area ========================================
            root.PackStart(stylesFrame, true, true, 0);
            root.PackStart(editorFrame, false, false, 0);
            root.PackEnd(actions, false, false, 0);

            // Dialogs don’t use Add(); pack into ContentArea
            var content = (Box)ContentArea;
            content.Spacing = 8;
            content.PackStart(root, true, true, 0);
        }

        void WireSignals()
        {
            // Standard close
            DeleteEvent += (_, a) => a.RetVal = false;
        }

        void PopulateFontFamilies()
        {
            var families = CreatePangoContext().Families;
            foreach (var name in families.Select(f => f.Name).OrderBy(n => n))
                CmbFont.AppendText(name);
            if (CmbFont.Model != null) CmbFont.Active = 0;
        }

        // ---------- Helpers ----------
        public void SetStyles(IEnumerable<(int id, string name)> items, int? selectId = null)
        {
            StylesStore.Clear();
            foreach (var (id, name) in items)
                StylesStore.AppendValues(id, name);

            if (selectId.HasValue && TryFindIterById(selectId.Value, out var it))
                StylesView.Selection.SelectIter(it);
        }

        public int? GetSelectedStyleId()
        {
            return StylesView.Selection.GetSelected(out TreeIter it)
                ? (int)StylesStore.GetValue(it, 0)
                : (int?)null;
        }

        bool TryFindIterById(int id, out TreeIter iter)
        {
            if (StylesStore.GetIterFirst(out iter))
            {
                do
                {
                    if ((int)StylesStore.GetValue(iter, 0) == id) return true;
                } while (StylesStore.IterNext(ref iter));
            }
            return false;
        }
        public Enums.FontDecorations GetDecorations()
        {
            Enums.FontDecorations f = Enums.FontDecorations.None;
            if (CbBold.Active) f |= Enums.FontDecorations.Bold;
            if (CbItalic.Active) f |= Enums.FontDecorations.Italic;
            if (CbUnderline.Active) f |= Enums.FontDecorations.Underline;
            if (CbStrike.Active) f |= Enums.FontDecorations.Strikethrough;
            return f;
        }

        public void SetDecorations(Enums.FontDecorations flags)
        {
            CbBold.Active = flags.HasFlag(Enums.FontDecorations.Bold);
            CbItalic.Active = flags.HasFlag(Enums.FontDecorations.Italic);
            CbUnderline.Active = flags.HasFlag(Enums.FontDecorations.Underline);
            CbStrike.Active = flags.HasFlag(Enums.FontDecorations.Strikethrough);
        }

        public static string ToHex(RGBA rgba) // for storing colors as "#RRGGBB"
        {
            byte r = (byte)(rgba.Red * 255);
            byte g = (byte)(rgba.Green * 255);
            byte b = (byte)(rgba.Blue * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
