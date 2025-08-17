using System;
using Gtk;
using Clingies.Gtk.Windows;
using Microsoft.Extensions.DependencyInjection;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Models;
using Clingies.ApplicationLogic.Services;

namespace Clingies.Gtk
{
    public sealed class GtkFrontendHost : IFrontendHost
    {
        public int Run(IServiceProvider services, string[] args)
        {
            Application.Init();
            var provider = new ClingyCssProvider();
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, StyleProviderPriority.Application);

            var clingyService = services.GetRequiredService<ClingyService>();
            var clingies = clingyService.GetAllActive(); // your actual method

            if (clingies.Count == 0)
            {
                var demo = new ClingyDto
                {
                    Title = "GTK 👋",
                    Content = "Prototype",
                    Width = 320,
                    Height = 220,
                    PositionX = 100,
                    PositionY = 100
                };
                demo.Id = clingyService.Create(demo);
                clingies = new() { demo };
            }

            foreach (var c in clingies)
            {
                var w = new ClingyWindow(c, clingyService);
                w.ShowAll();
            }

            Application.Run();
            return 0;
        }
    }
}