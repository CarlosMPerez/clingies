using System.Linq;
using Gtk;
using Gdk;
using Clingies.Application.Services;
using System;
using Clingies.Domain.Models;

namespace Clingies.GtkFront.Windows
{
 public sealed class StyleManagerDialog : Dialog
    {
        private const int Height  = 480;

        // State
        private bool _isExpanded;
        private const int SystemStyleId = 1;

        // Hidden selection id (empty for New)
        public Entry HiddenStyleId { get; private set; }

        // Styles list
        private TreeView StylesView;
        private ListStore StylesStore; // id(int), name(string), is_system(bool)

        // Toolbar bits
        private Button BtnDelete;
        private Button BtnNew;

        // Editor container (shown only when expanded)
        private Frame EditorFrame;
        private Grid EditorGrid;

        // Editor controls
        private Entry EntryStyleName;
        private CheckButton ChkActive;
        private CheckButton ChkDefault;
        private ComboBoxText CmbFont;
        private SpinButton SpinFontSize;
        private ColorButton BtnFontColor;
        private ColorButton BtnBodyColor;
        private CheckButton CbBold;
        private CheckButton CbItalic;
        private CheckButton CbUnderline;
        private CheckButton CbStrike;

        // Footer buttons
        private Button BtnCancel;
        private Button BtnSave;

        private readonly StyleService _styleService;

        public StyleManagerDialog(StyleService styleService, Gtk.Window parent = null)
            : base("Style Manager", parent, DialogFlags.Modal)
        {
            _styleService = styleService;
            // Dialog chrome/behavior
            DefaultWidth = 560;
            DefaultHeight = Height;
            BorderWidth = 8;
            TypeHint = WindowTypeHint.Dialog;
            Resizable = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;
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
            LoadStyles();

            ShowAll();
        }

        // --------------------- UI BUILD ---------------------
        private void BuildUi()
        {
            var root = new Box(Orientation.Vertical, 8);

            // Hidden id
            HiddenStyleId = new Entry
            {
                Name = "style_id_hidden",
                Visible = false,
                NoShowAll = true,
                Sensitive = false
            };
            root.PackStart(HiddenStyleId, false, false, 0);

            // Top bar: "Styles" label + Delete icon on the right
            var topBar = new Box(Orientation.Horizontal, 8);
            var lblStyles = new Label("Styles") { Xalign = 0 };
            topBar.PackStart(lblStyles, true, true, 0);

            BtnDelete = new Button { TooltipText = "Delete selected style" };
            // icon
            var trash = new Image();
            try
            {
                // Try themed icon first
                trash = Image.NewFromIconName("user-trash", IconSize.Button);
            }
            catch { /* fallback below if needed */ }
            if (trash == null || trash.Handle == IntPtr.Zero)
                trash = new Image(Stock.Delete, IconSize.Button);
            BtnDelete.Image = trash;
            topBar.PackEnd(BtnDelete, false, false, 0);

            root.PackStart(topBar, false, false, 0);

            // Styles list
            var stylesFrame = new Frame();
            var scStyles = new ScrolledWindow();
            StylesView = new TreeView { HeadersVisible = false };
            StylesStore = new ListStore(typeof(int), typeof(string), typeof(bool));
            StylesView.Model = StylesStore;

            var col = new TreeViewColumn();
            var cell = new CellRendererText();
            col.PackStart(cell, true);
            col.AddAttribute(cell, "text", 1);
            StylesView.AppendColumn(col);
            StylesView.Selection.Mode = SelectionMode.Single;

            scStyles.Add(StylesView);
            stylesFrame.Add(scStyles);
            root.PackStart(stylesFrame, true, true, 0);

            // Editor (hidden in collapsed mode)
            EditorFrame = new Frame { Label = "Style" };
            EditorGrid = new Grid { ColumnSpacing = 8, RowSpacing = 8, Margin = 8 };

            int r = 0;
            EditorGrid.Attach(new Label("Style Name") { Xalign = 0 }, 0, r, 1, 1);
            EntryStyleName = new Entry();
            EditorGrid.Attach(EntryStyleName, 1, r, 2, 1);

            var flagsBox = new Box(Orientation.Horizontal, 8);
            ChkActive  = new CheckButton("Active");
            ChkDefault = new CheckButton("Default");
            flagsBox.PackStart(ChkActive, false, false, 0);
            flagsBox.PackStart(ChkDefault, false, false, 0);
            EditorGrid.Attach(flagsBox, 3, r, 1, 1);

            r++;
            EditorGrid.Attach(new Label("Font") { Xalign = 0 }, 0, r, 1, 1);
            CmbFont = new ComboBoxText();
            EditorGrid.Attach(CmbFont, 1, r, 3, 1);

            r++;
            EditorGrid.Attach(new Label("Font Size") { Xalign = 0 }, 0, r, 1, 1);
            SpinFontSize = new SpinButton(new Adjustment(12, 6, 144, 1, 5, 0), 1, 0);
            EditorGrid.Attach(SpinFontSize, 1, r, 1, 1);

            EditorGrid.Attach(new Label("Font Color") { Xalign = 1 }, 2, r, 1, 1);
            BtnFontColor = new ColorButton();
            EditorGrid.Attach(BtnFontColor, 3, r, 1, 1);

            r++;
            EditorGrid.Attach(new Label("Body Color") { Xalign = 1 }, 2, r, 1, 1);
            BtnBodyColor = new ColorButton();
            EditorGrid.Attach(BtnBodyColor, 3, r, 1, 1);

            r++;
            EditorGrid.Attach(new Label("Font Decorations") { Xalign = 0 }, 0, r, 1, 1);
            var decoBox = new Box(Orientation.Horizontal, 12);
            CbBold      = new CheckButton("Bold");
            CbItalic    = new CheckButton("Italics");
            CbUnderline = new CheckButton("Underline");
            CbStrike    = new CheckButton("Strikethrough");
            decoBox.PackStart(CbBold, false, false, 0);
            decoBox.PackStart(CbItalic, false, false, 0);
            decoBox.PackStart(CbUnderline, false, false, 0);
            decoBox.PackStart(CbStrike, false, false, 0);
            EditorGrid.Attach(decoBox, 1, r, 3, 1);

            EditorFrame.Add(EditorGrid);
            root.PackStart(EditorFrame, false, false, 0);

            // Footer: New | Cancel / Save
            var footer = new Box(Orientation.Horizontal, 8);

            BtnNew = new Button("New");
            footer.PackStart(BtnNew, false, false, 0);

            var spacer = new Label(""); // pushes Cancel/Save to the right
            footer.PackStart(spacer, true, true, 0);

            BtnCancel = new Button("Cancel");
            BtnSave   = new Button("Save");
            BtnSave.StyleContext.AddClass("suggested-action");
            footer.PackEnd(BtnSave,   false, false, 0);
            footer.PackEnd(BtnCancel, false, false, 0);

            root.PackEnd(footer, false, false, 0);

            // Pack into dialog
            var content = (Box)ContentArea;
            content.Spacing = 8;
            content.PackStart(root, true, true, 0);
        }

        private void WireSignals()
        {
            DeleteEvent += (_, a) =>
            {
                a.RetVal = false; // default close
            };

            StylesView.Selection.Changed += OnStylesSelectionChanged;
            BtnNew.Clicked    += OnNewClicked;
            BtnDelete.Clicked += OnDeleteClicked;
            BtnSave.Clicked   += OnSaveClicked;
            BtnCancel.Clicked += OnCancelClicked;
        }

        private void PopulateFontFamilies()
        {
            var families = CreatePangoContext().Families;
            foreach (var name in families.Select(f => f.Name).OrderBy(n => n))
                CmbFont.AppendText(name);
            if (CmbFont.Model != null) CmbFont.Active = 0;
        }

        private void LoadStyles(int? selectId = null)
        {

            StylesStore.Clear();
            foreach (var s in _styleService.GetAll())
                StylesStore.AppendValues(s.Id, s.StyleName, s.IsSystem);

            // selection
            TreeIter iter;
            if (selectId.HasValue && TryFindIterById(selectId.Value, out iter))
            {
                StylesView.Selection.SelectIter(iter);
            }
            else
            {
                StylesView.Selection.UnselectAll();
            }

            // enable/disable delete button
            UpdateDeleteSensitivity();
        }

        // --------------------- BEHAVIOR ---------------------
        private void OnNewClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("[StyleManager] New clicked -> expanded editor with defaults.");
            StylesView.Selection.UnselectAll();
            HiddenStyleId.Text = "";  // no id
            LoadEditorDefaults();
            SetEditorReadOnly(false);
            UpdateDeleteSensitivity();
        }

        private void OnDeleteClicked(object? sender, EventArgs e)
        {
            if (!StylesView.Selection.GetSelected(out TreeIter it))
                return;

            int id   = (int)StylesStore.GetValue(it, 0);
            string n = (string)StylesStore.GetValue(it, 1);
            bool isSystem = (bool)StylesStore.GetValue(it, 2);

            if (isSystem)
            {
                Console.WriteLine("[StyleManager] Refusing to delete System style.");
                return;
            }

            using var md = new MessageDialog(this, DialogFlags.Modal, MessageType.Question, ButtonsType.OkCancel,
                $"Delete style “{n}”?");
            var resp = (ResponseType)md.Run();
            md.Hide();

            if (resp == ResponseType.Ok)
            {
                Console.WriteLine($"[StyleManager] Deleting style id={id} (mock).");
                _styleService.Delete(id);
                // reload list, collapse editor
                LoadStyles();
            }
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            var idText = HiddenStyleId.Text;

            if (string.IsNullOrWhiteSpace(idText))
            {
                // create
                StyleModel model = ReadUiIntoModel();
                Console.WriteLine($"[StyleManager] Creating style -> {model.Id}, {model.StyleName}");
                _styleService.Create(model);
                LoadStyles(model.Id);
            }
            else
            {
                // update
                int id = int.Parse(idText);
                var existing = ReadUiIntoModel();
                if (existing != null)
                {
                    Console.WriteLine($"[StyleManager] Updating style id={id}.");
                    _styleService.Update(existing);
                }
                LoadStyles(id);
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("[StyleManager] Cancel -> close dialog.");
            Respond(ResponseType.Cancel); // same as clicking X
            Destroy();
        }

        private void OnStylesSelectionChanged(object? sender, EventArgs e)
        {
            if (!StylesView.Selection.GetSelected(out TreeIter it))
            {
                UpdateDeleteSensitivity();
                return;
            }

            int id = (int)StylesStore.GetValue(it, 0);
            bool isSystem = (bool)StylesStore.GetValue(it, 2);

            HiddenStyleId.Text = id.ToString();
            LoadStyle(id);
            SetEditorReadOnly(isSystem);
            UpdateDeleteSensitivity();
        }

        // --------------------- UTILITIES ---------------------OnStyle
        private StyleModel ReadUiIntoModel()
        {
            // Hidden id: empty = new
            int id = 0;
            if (int.TryParse(HiddenStyleId.Text, out var parsed)) id = parsed;

            var model = new StyleModel
            {
                Id         = id,
                StyleName       = (EntryStyleName.Text ?? string.Empty).Trim(),
                IsActive   = ChkActive.Active,
                IsDefault  = ChkDefault.Active,
                BodyFontName   = CmbFont.ActiveText ?? "Sans",
                BodyFontSize   = SpinFontSize.ValueAsInt,
                BodyFontColor  = ToHex(BtnFontColor.Rgba),
                BodyColor  = ToHex(BtnBodyColor.Rgba),
                BodyFontDecorations = GetDecorations(),
                // For the mock we infer System by id; in real code, fetch IsSystem from DB.
                IsSystem   = id == SystemStyleId
            };

            return model;
        }

        private void SetEditorReadOnly(bool readOnly)
        {
            EntryStyleName.Sensitive = !readOnly;
            ChkActive.Sensitive      = !readOnly;
            ChkDefault.Sensitive     = !readOnly;
            CmbFont.Sensitive        = !readOnly;
            SpinFontSize.Sensitive   = !readOnly;
            BtnFontColor.Sensitive   = !readOnly;
            BtnBodyColor.Sensitive   = !readOnly;
            CbBold.Sensitive         = !readOnly;
            CbItalic.Sensitive       = !readOnly;
            CbUnderline.Sensitive    = !readOnly;
            CbStrike.Sensitive       = !readOnly;

            BtnSave.Sensitive = !readOnly; // cannot save System
        }

        private void UpdateDeleteSensitivity()
        {
            bool canDelete = false;
            if (StylesView.Selection.GetSelected(out TreeIter it))
            {
                bool isSystem = (bool)StylesStore.GetValue(it, 2);
                canDelete = !isSystem;
            }
            BtnDelete.Sensitive = canDelete;
        }

        private void LoadEditorDefaults()
        {
            EntryStyleName.Text = "";
            ChkActive.Active  = true;
            ChkDefault.Active = false;

            // Best effort defaults
            CmbFont.Active = 0;
            SpinFontSize.Value = 12;

            BtnFontColor.Rgba = ParseHex("#000000");
            BtnBodyColor.Rgba = ParseHex("#FFFFB8");

            SetDecorations(Enums.FontDecorations.None);
        }

        private void LoadStyle(int id)
        {
            var style = _styleService.Get(id);
            if (style == null) { LoadEditorDefaults(); return; }

            EntryStyleName.Text = style.StyleName;
            ChkActive.Active  = style.IsActive;
            ChkDefault.Active = style.IsDefault;

            // Font combo best-effort match
            SetComboActiveText(CmbFont, style.BodyFontName);
            SpinFontSize.Value = style.BodyFontSize;

            BtnFontColor.Rgba = ParseHex(style.BodyFontColor);
            BtnBodyColor.Rgba = ParseHex(style.BodyColor);

            SetDecorations(style.BodyFontDecorations);
        }

        private void SetComboActiveText(ComboBoxText combo, string text)
        {
            // Linear search; good enough for UX
            TreeIter it;
            if (combo.Model != null && combo.Model.GetIterFirst(out it))
            {
                do
                {
                    var val = combo.Model.GetValue(it, 0).ToString();
                    if (string.Equals(val, text, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SetActiveIter(it);
                        return;
                    }
                } while (combo.Model.IterNext(ref it));
            }
            combo.Active = 0;
        }

        private Enums.FontDecorations GetDecorations()
        {
            Enums.FontDecorations f = Enums.FontDecorations.None;
            if (CbBold.Active)      f |= Enums.FontDecorations.Bold;
            if (CbItalic.Active)    f |= Enums.FontDecorations.Italic;
            if (CbUnderline.Active) f |= Enums.FontDecorations.Underline;
            if (CbStrike.Active)    f |= Enums.FontDecorations.Strikethrough;
            return f;
        }

        private void SetDecorations(Enums.FontDecorations flags)
        {
            CbBold.Active      = flags.HasFlag(Enums.FontDecorations.Bold);
            CbItalic.Active    = flags.HasFlag(Enums.FontDecorations.Italic);
            CbUnderline.Active = flags.HasFlag(Enums.FontDecorations.Underline);
            CbStrike.Active    = flags.HasFlag(Enums.FontDecorations.Strikethrough);
        }

        public static string ToHex(RGBA rgba)
        {
            byte r = (byte)(rgba.Red * 255);
            byte g = (byte)(rgba.Green * 255);
            byte b = (byte)(rgba.Blue * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static RGBA ParseHex(string hex)
        {
            var rgba = new RGBA();
            try { rgba.Parse(hex); }
            catch
            {
                rgba.Red = rgba.Green = rgba.Blue = 0; rgba.Alpha = 1;
            }
            return rgba;
        }

        private bool TryFindIterById(int id, out TreeIter iter)
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
    }    
}
