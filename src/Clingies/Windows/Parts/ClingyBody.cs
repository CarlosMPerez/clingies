using System;
using System.IO;
using Clingies.Domain.Models;
using Gtk;

namespace Clingies.Windows.Parts;

public sealed class ClingyBody : Overlay
{
    private const int ImagePadding = 20;

    private TextView _content;
    private ScrolledWindow _scroller;
    private EventBox _imageHost;
    private Image _image;
    private EventBox _leftGrip;
    private EventBox _rightGrip;
    private ClingyWindowCallbacks? _callbacks;
    private bool _isLocked;
    private bool _hasImageContent;
    private bool _imagePastePending;
    private byte[]? _imageBytes;
    private int _imageWidth;
    private int _imageWindowHeight;

    private bool _autoSizeEnabled = true;
    private uint _autosizeId;
    private int _lastAutoHeight = -1;
    private bool _scrollNormalizationPending;
    private uint _scrollNormalizationId;
    private bool _contentFitsWithoutScrolling = true;
    private bool _pasteScrollRecoveryPending;

    private ClingyBody() : base()
    {
        _content = new TextView
        {
            Name = AppConstants.CssSections.ClingyContentView,
            CursorVisible = true,
            Editable = true,
            CanFocus = true,
            WrapMode = Gtk.WrapMode.WordChar,
            LeftMargin = 6,
            RightMargin = 6
        };

        _scroller = new ScrolledWindow();
        _image = new Image
        {
            Halign = Align.Center,
            Valign = Align.Center,
            MarginStart = ImagePadding,
            MarginEnd = ImagePadding,
            MarginTop = ImagePadding,
            MarginBottom = ImagePadding
        };
        _imageHost = new EventBox
        {
            VisibleWindow = false,
            CanFocus = true
        };
        _imageHost.Add(_image);
        _leftGrip = new EventBox();
        _rightGrip = new EventBox();
    }

    public static ClingyBody Build(ClingyModel model,
                                    Gtk.Window owner,
                                    ClingyWindowCallbacks cb)
    {
        var overlay = new ClingyBody();
        overlay._callbacks = cb;
        overlay._scroller = new ScrolledWindow
        {
            Name = AppConstants.CssSections.ClingyContent,
            ShadowType = ShadowType.None,
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic
        };


        // init + caret behavior
        overlay._content.Buffer.Text = model.Text ?? string.Empty;
        overlay.InitializeContentFromModel(model);
        overlay._content.MapEvent += (_, __) => GLib.Idle.Add(() =>
        {
            if (overlay._hasImageContent) overlay._imageHost.GrabFocus();
            else overlay._content.GrabFocus();
            return false;
        });
        overlay._content.EnterNotifyEvent += (_, e) =>
        {
            try
            {
                var win = e.Event.Window ?? overlay._content.GetWindow(TextWindowType.Text);
                using var cursor = new Gdk.Cursor(owner.Display, Gdk.CursorType.Xterm);
                if (win != null) win.Cursor = cursor;
            }
            catch { }
        };

        overlay._content.LeaveNotifyEvent += (_, e) =>
        {
            try { (e.Event.Window ?? overlay._content.GetWindow(TextWindowType.Text))!.Cursor = null; } catch { }
        };

        // content changes bubble up
        overlay._content.Buffer.Changed += (_, e) =>
        {
            cb.ContentChanged(overlay._content.Buffer.Text);
            overlay.DebounceAutosize(owner);
        };
        overlay._imageHost.AddEvents((int)(Gdk.EventMask.ButtonPressMask | Gdk.EventMask.KeyPressMask));
        overlay._imageHost.ButtonPressEvent += (_, __) => overlay._imageHost.GrabFocus();
        overlay._imageHost.KeyPressEvent += (_, e) => overlay.HandleImageKeyPress(owner, e);

        overlay._content.SizeAllocated += (_, __) =>
        {
            overlay.DebounceAutosize(owner);
            overlay.FlushPendingScrollNormalization();
        };
        overlay._scroller.SizeAllocated += (_, __) => overlay.FlushPendingScrollNormalization();
        overlay.MapEvent += (_, __) => overlay.DebounceAutosize(owner);

        overlay._scroller.Add(overlay._content);
        overlay.Add(overlay._scroller);
        overlay.AddOverlay(overlay._imageHost);
        overlay.UpdateVisibleContent();

        // grips: 2 thin overlays that start resize drags, and on release report size+pos
        overlay._leftGrip = MakeGrip(owner, cb, true, () => overlay._isLocked, () => overlay.DebounceAutosize(owner));
        overlay._rightGrip = MakeGrip(owner, cb, false, () => overlay._isLocked, () => overlay.DebounceAutosize(owner));
        overlay.AddOverlay(overlay._leftGrip);
        overlay.AddOverlay(overlay._rightGrip);

        return overlay;
    }

    public void ApplyLock(bool isLocked, Gtk.Window owner)
    {
        _isLocked = isLocked;
        ApplyInteractivity(owner);

        if (isLocked)
        {
            _content.KeyPressEvent += BlockKey;
            _content.ButtonPressEvent += BlockMouse;
            _content.PasteClipboard += BlockClipboard;
            _content.CutClipboard += BlockClipboard;
            _content.Buffer.InsertText += BlockInsert;
            _content.Buffer.DeleteRange += BlockDelete;
        }
        else
        {
            _content.KeyPressEvent -= BlockKey;
            _content.ButtonPressEvent -= BlockMouse;
            _content.PasteClipboard -= BlockClipboard;
            _content.CutClipboard -= BlockClipboard;
            _content.Buffer.InsertText -= BlockInsert;
            _content.Buffer.DeleteRange -= BlockDelete;
        }

        // Optional: neutral cursor when locked
        if (isLocked)
        {
            _leftGrip.Window?.SetDeviceCursor(owner.Display.DefaultSeat.Pointer, null);
            _rightGrip.Window?.SetDeviceCursor(owner.Display.DefaultSeat.Pointer, null);
        }
    }

    public void ApplyRollState(Gtk.Window owner, Box parent, bool isRolled, int width)
    {
        if (isRolled)
        {
            SetAutoSizeEnabled(false);
            if (_hasImageContent)
                UnloadImageView();
            HeightRequest = 0;
            Hide();
            parent.SetChildPacking(this, expand: false, fill: false, padding: 0, PackType.Start);
            var targetWidth = owner.Allocation.Width > 0 ? owner.Allocation.Width : width;
            parent.SetSizeRequest(-1, AppConstants.Dimensions.TitleHeight);
            owner.SetSizeRequest(targetWidth, AppConstants.Dimensions.TitleHeight);
            parent.QueueResize();
            QueueRolledResize(owner, targetWidth);
        }
        else
        {
            SetAutoSizeEnabled(!_hasImageContent);
            HeightRequest = -1;
            parent.SetSizeRequest(-1, -1);
            owner.SetSizeRequest(-1, -1);
            parent.SetChildPacking(this, expand: true, fill: true, padding: 0, PackType.Start);
            if (_hasImageContent)
                EnsureImageViewLoaded();
            ShowAll();
            UpdateVisibleContent();
            if (_hasImageContent)
            {
                ApplyInteractivity(owner);
                ClampToFixedSize(owner, _imageWidth, _imageWindowHeight);
                ResizeKeepingPosition(owner, _imageWidth, _imageWindowHeight, forceResizable: true);
            }
            else
            {
                UnclampHeight(owner);
                DebounceAutosize(owner);
                ApplyInteractivity(owner);
            }
        }

        parent.QueueResize();
    }

    public void SetAutoSizeEnabled(bool enabled)
    {
        _autoSizeEnabled = enabled;

        if (!enabled && _autosizeId != 0)
        {
            GLib.Source.Remove(_autosizeId);
            _autosizeId = 0;
        }
    }

    private static EventBox MakeGrip(Window owner, ClingyWindowCallbacks cb, bool isLeft, Func<bool> isLocked,
        System.Action autosizeAfterResize)
    {
        var grip = new EventBox
        {
            VisibleWindow = false,
            WidthRequest = 6,
            Halign = isLeft ? Align.Start : Align.End,
            Valign = Align.Fill
        };
        grip.Events |= Gdk.EventMask.ButtonPressMask |
                        Gdk.EventMask.ButtonReleaseMask |
                        Gdk.EventMask.EnterNotifyMask |
                        Gdk.EventMask.LeaveNotifyMask;

        grip.ButtonPressEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            if (e.Event.Button == 1)
                owner.BeginResizeDrag(isLeft ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                                      (int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        grip.ButtonReleaseEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            // end-of-resize certainty
            cb.SizeChanged(owner.Allocation.Width, owner.Allocation.Height);
            owner.GetPosition(out var x, out var y);
            cb.PositionChanged(x, y);
            autosizeAfterResize();
        };
        grip.EnterNotifyEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            try
            {
                using var c = new Gdk.Cursor(owner.Display, isLeft ? Gdk.CursorType.LeftSide : Gdk.CursorType.RightSide);
                (e.Event.Window ?? grip.Window)!.Cursor = c;
            }
            catch { }
        };
        grip.LeaveNotifyEvent += (_, e) =>
        {
            try { (e.Event.Window ?? grip.Window)!.Cursor = null; } catch { }
        };

        return grip;
    }

    private void DebounceAutosize(Gtk.Window owner)
    {
        if (!_autoSizeEnabled) return;

        if (_autosizeId != 0) GLib.Source.Remove(_autosizeId);
        _autosizeId = GLib.Timeout.Add(16, () =>
        {
            _autosizeId = 0;
            AutoHeightToContent(owner);
            return false;
        });
    }

    private void AutoHeightToContent(Gtk.Window owner)
    {
        int availableTextWidth = _content.Allocation.Width - _content.LeftMargin - _content.RightMargin;
        if (availableTextWidth <= 0) return;

        var text = string.IsNullOrEmpty(_content.Buffer.Text) ? " " : _content.Buffer.Text;
        using var layout = _content.CreatePangoLayout(text);
        layout.Wrap = Pango.WrapMode.WordChar;
        layout.Width = (int)(availableTextWidth * Pango.Scale.PangoScale);
        layout.GetPixelSize(out _, out int textH);

        const int MinWindowHeight = AppConstants.Dimensions.DefaultClingyHeight;
        const int MaxWindowHeight = 1500;
        const int TextVerticalPad = 16;

        int chromeHeight = owner.Allocation.Height > 0 && _content.Allocation.Height > 0
            ? Math.Max(AppConstants.Dimensions.TitleHeight, owner.Allocation.Height - _content.Allocation.Height)
            : AppConstants.Dimensions.TitleHeight;

        int desiredWindowHeight = chromeHeight + textH + TextVerticalPad;
        _contentFitsWithoutScrolling = desiredWindowHeight <= MaxWindowHeight;
        int targetH = Math.Max(MinWindowHeight, Math.Min(MaxWindowHeight, desiredWindowHeight));
        int currentW = owner.Allocation.Width > 0 ? owner.Allocation.Width : AppConstants.Dimensions.DefaultClingyWidth;

        if (_lastAutoHeight == targetH && owner.Allocation.Height == targetH)
        {
            QueueScrollNormalization();
            return;
        }

        if (Math.Abs(owner.Allocation.Height - targetH) <= 1)
        {
            _lastAutoHeight = targetH;
            QueueScrollNormalization();
            return;
        }

        _lastAutoHeight = targetH;
        _scrollNormalizationPending = true;
        ResizeKeepingPosition(owner, currentW, targetH);
    }

    private void QueueScrollNormalization()
    {
        if (_scrollNormalizationId != 0)
            GLib.Source.Remove(_scrollNormalizationId);

        GLib.Idle.Add(() =>
        {
            NormalizeScrollPosition();
            return false;
        });

        // Gtk may update adjustment/page size one or two frames after the resize.
        // Retry shortly after the idle pass so a first Ctrl+V lands in the final settled state.
        _scrollNormalizationId = GLib.Timeout.Add(48, () =>
        {
            NormalizeScrollPosition();
            _scrollNormalizationId = GLib.Timeout.Add(120, () =>
            {
                _scrollNormalizationId = 0;
                NormalizeScrollPosition();
                _pasteScrollRecoveryPending = false;
                return false;
            });
            return false;
        });
    }

    private void NormalizeScrollPosition()
    {
        var vadj = _scroller.Vadjustment;
        if (vadj == null) return;

        if (_pasteScrollRecoveryPending || _contentFitsWithoutScrolling)
        {
            if (Math.Abs(vadj.Value - vadj.Lower) > 1)
                vadj.Value = vadj.Lower;
            return;
        }

        var insertMark = _content.Buffer.InsertMark;
        _content.ScrollMarkOnscreen(insertMark);
    }

    private void FlushPendingScrollNormalization()
    {
        if (!_scrollNormalizationPending) return;

        _scrollNormalizationPending = false;
        QueueScrollNormalization();
    }

    private void HandleImageKeyPress(Gtk.Window owner, KeyPressEventArgs e)
    {
        if (!IsCtrlV(e))
            return;

        if (ClipboardHasImage(owner))
            e.RetVal = true;
    }

    public bool TryPasteImageFromClipboard(Gtk.Window owner)
    {
        if (_imagePastePending)
            return true;

        var clipboard = GetClipboard(owner);
        var pixbuf = clipboard.WaitForImage();
        if (pixbuf is null)
            return false;

        if (_isLocked || HasAnyContent())
            return true;

        _imagePastePending = true;
        GLib.Idle.Add(() =>
        {
            _imagePastePending = false;
            ShowImage(owner, pixbuf);
            return false;
        });
        return true;
    }

    private void ShowImage(Gtk.Window owner, Gdk.Pixbuf pixbuf)
    {
        var blob = SerializePixbuf(pixbuf);
        _imageBytes = blob;
        _hasImageContent = true;
        _imageWidth = pixbuf.Width + (ImagePadding * 2);
        _image.Pixbuf = pixbuf;
        UpdateVisibleContent();
        SetAutoSizeEnabled(false);

        var chromeHeight = GetChromeHeight(owner);
        _imageWindowHeight = chromeHeight + pixbuf.Height + (ImagePadding * 2);

        ApplyInteractivity(owner);
        ClampToFixedSize(owner, _imageWidth, _imageWindowHeight);
        ResizeKeepingPosition(owner, _imageWidth, _imageWindowHeight, forceResizable: true);

        GLib.Idle.Add(() =>
        {
            _imageHost.GrabFocus();
            return false;
        });

        _callbacks?.ImageChanged(blob);
    }

    private void ApplyInteractivity(Gtk.Window owner)
    {
        var canResize = !_isLocked && !_hasImageContent;

        owner.Resizable = canResize;
        _leftGrip.Sensitive = canResize;
        _rightGrip.Sensitive = canResize;

        _content.Editable = !_isLocked && !_hasImageContent;
        _content.CursorVisible = !_isLocked && !_hasImageContent;
        _content.CanFocus = !_isLocked && !_hasImageContent;
        _imageHost.CanFocus = !_isLocked && _hasImageContent;
    }

    private void UpdateVisibleContent()
    {
        if (_hasImageContent)
        {
            _scroller.Hide();
            _imageHost.ShowAll();
        }
        else
        {
            _imageHost.Hide();
            _scroller.ShowAll();
        }
    }

    private void InitializeContentFromModel(ClingyModel model)
    {
        if (model.PngBytes is not { Length: > 0 })
            return;

        _imageBytes = model.PngBytes;
        var pixbuf = DeserializePixbuf(model.PngBytes);
        if (pixbuf is null)
            return;

        _hasImageContent = true;
        _image.Pixbuf = pixbuf;
        _imageWidth = model.Width > 0 ? (int)model.Width : pixbuf.Width + (ImagePadding * 2);
        _imageWindowHeight = model.Height > 0
            ? (int)model.Height
            : AppConstants.Dimensions.TitleHeight + pixbuf.Height + (ImagePadding * 2);
        SetAutoSizeEnabled(false);
    }

    private void UnloadImageView()
    {
        if (!_hasImageContent)
            return;

        _image.Pixbuf = null;
        _image.SetSizeRequest(1, 1);
        _imageHost.SetSizeRequest(1, 1);
        _image.Hide();
        _imageHost.Hide();
    }

    private void EnsureImageViewLoaded()
    {
        if (!_hasImageContent)
            return;

        _image.SetSizeRequest(-1, -1);
        _imageHost.SetSizeRequest(-1, -1);

        if (_image.Pixbuf is not null)
            return;

        if (_imageBytes is not { Length: > 0 })
            return;

        var pixbuf = DeserializePixbuf(_imageBytes);
        if (pixbuf is null)
            return;

        _image.Pixbuf = pixbuf;
    }

    private static byte[] SerializePixbuf(Gdk.Pixbuf pixbuf)
    {
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            pixbuf.Save(tempPath, "png");
            return System.IO.File.ReadAllBytes(tempPath);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    private static Gdk.Pixbuf? DeserializePixbuf(byte[] bytes)
    {
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            System.IO.File.WriteAllBytes(tempPath, bytes);
            return new Gdk.Pixbuf(tempPath);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    private bool HasAnyContent() =>
        _hasImageContent || _content.Buffer.CharCount > 0;

    private static bool IsCtrlV(KeyPressEventArgs e) =>
        (e.Event.State & Gdk.ModifierType.ControlMask) != 0 &&
        (e.Event.Key == Gdk.Key.v || e.Event.Key == Gdk.Key.V);

    private Clipboard GetClipboard(Gtk.Window owner) =>
        Clipboard.GetForDisplay(owner.Display, Gdk.Atom.Intern("CLIPBOARD", false));

    private bool ClipboardHasImage(Gtk.Window owner) =>
        GetClipboard(owner).WaitForImage() is not null;

    private int GetChromeHeight(Gtk.Window owner) =>
        owner.Allocation.Height > 0 && _scroller.Allocation.Height > 0
            ? Math.Max(AppConstants.Dimensions.TitleHeight, owner.Allocation.Height - _scroller.Allocation.Height)
            : AppConstants.Dimensions.TitleHeight;

    private static void ResizeKeepingPosition(Gtk.Window owner, int width, int height, bool forceResizable = false)
    {
        var previousResizable = owner.Resizable;
        if (forceResizable)
            owner.Resizable = true;

        owner.GetPosition(out var x, out var y);
        owner.Resize(width, height);

        GLib.Idle.Add(() =>
        {
            owner.Move(x, y);
            if (forceResizable)
                owner.Resizable = previousResizable;
            return false;
        });

        GLib.Timeout.Add(48, () =>
        {
            owner.Move(x, y);
            if (forceResizable)
                owner.Resizable = previousResizable;
            return false;
        });
    }

    private void QueueRolledResize(Gtk.Window owner, int width)
    {
        GLib.Idle.Add(() =>
        {
            ClampToTitleBarHeight(owner, width);
            ResizeKeepingPosition(owner, width, AppConstants.Dimensions.TitleHeight, forceResizable: true);
            return false;
        });

        GLib.Timeout.Add(48, () =>
        {
            ClampToTitleBarHeight(owner, width);
            ResizeKeepingPosition(owner, width, AppConstants.Dimensions.TitleHeight, forceResizable: true);
            return false;
        });
    }

    private void ClampToTitleBarHeight(Gtk.Window owner, int width)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = AppConstants.Dimensions.TitleHeight,
            MaxHeight = AppConstants.Dimensions.TitleHeight,
            MinWidth = width,
            MaxWidth = width
        };
        owner.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
    }

    private void ClampToFixedSize(Gtk.Window owner, int width, int height)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = height,
            MaxHeight = height,
            MinWidth = width,
            MaxWidth = width
        };
        owner.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
    }

    private void UnclampHeight(Gtk.Window owner)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = AppConstants.Dimensions.TitleHeight,
            MaxHeight = int.MaxValue,
            MinWidth = AppConstants.Dimensions.MinimumClingyWidth,
            MaxWidth = int.MaxValue
        };

        owner.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
    }

    private void BlockKey(object? o, Gtk.KeyPressEventArgs ev) => ev.RetVal = true;
    private void BlockMouse(object? o, Gtk.ButtonPressEventArgs ev) => ev.RetVal = true;
    private void BlockClipboard(object? o, EventArgs e) { }
    private void BlockInsert(object? o, Gtk.InsertTextArgs args) => args.RetVal = true;
    private void BlockDelete(object? o, Gtk.DeleteRangeArgs args) => args.RetVal = true;

}
