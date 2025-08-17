using System;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Models;
using Gtk;

namespace Clingies.Gtk.Windows
{

    public class ClingyWindow : Window
    {
        public ClingyWindow(ClingyDto dto, ClingyService service) : base(dto.Title ?? "")
        {
            Decorated = false; // no system title bar
            SkipTaskbarHint = true; // do not show in taskbar
            SkipPagerHint = true;

            // KeepAbove = true // future reference for Pin/Unpin

            DefaultWidth = (int)Math.Max(200, dto.Width);
            DefaultHeight = (int)Math.Max(120, dto.Height);
            Move((int)dto.PositionX, (int)dto.PositionY);

            var root = new Box(Orientation.Vertical, 0)
            {
                Name = "clingy-window",
                BorderWidth = 0
            };

            // Header (fake title bar)
            var header = new Box(Orientation.Horizontal, 6)
            {
                Name = "clingy-title",
                HeightRequest = 30,
                BorderWidth = 6
            };

            var titleEntry = new Entry(dto.Title ?? string.Empty)
            {
                Name = "clingy-title-entry",
                HasFrame = false
            };
            titleEntry.Changed += (_, __) => { dto.Title = titleEntry.Text; Title = titleEntry.Text; };

            // optional close button on the right
            var closeBtn = new Button("✕") { Name = "btn-close" };
            closeBtn.Clicked += (_, __) => this.Close();

            header.PackStart(titleEntry, true, true, 0);
            header.PackEnd(closeBtn, false, false, 6);

            // Body
            var scroller = new ScrolledWindow()
            {
                Name = "clingy-content",
                ShadowType = ShadowType.None
            };

            var content = new TextView();
            content.Name = "clingy-content-view";
            content.Buffer.Text = dto.Content ?? "";
            content.Buffer.Changed += (_, __) => { dto.Content = content.Buffer.Text; };

            scroller.Add(content);

            root.PackStart(header, false, false, 0);
            root.PackStart(scroller, true, true, 0);
            Add(root);

            // Wire events
            ConfigureEvent += (_, e) =>
            {
                dto.PositionX = e.Event.X; dto.PositionY = e.Event.Y;
                dto.Width = e.Event.Width; dto.Height = e.Event.Height;
            };

            DeleteEvent += (_, __) => service.Update(dto);

        }
    }
}