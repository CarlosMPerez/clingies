using System.Collections.Generic;
using Clingies.Application.Services;
using Clingies.Domain.Models;
using Clingies.App.Windows;
using System;
using Clingies.App.Windows.CustomEventArgs;
using System.Linq;

namespace Clingies.App.Factories;

public class ClingyWindowFactory(ClingyService noteService)
{
    private List<Clingy> activeClingies = new List<Clingy>();
    private List<ClingyWindow> activeWindows = new List<ClingyWindow>();
    public void CreateNewWindow(double posX, double posY)
    {
        var clingy = noteService.Create("", "", posX, posY);
        var window = new ClingyWindow(clingy);
        // subscribe to events
        window.CloseRequested += HandleCloseRequested;
        window.PinRequested += HandlePinRequested;
        window.PositionChangeRequested += HandlePositionChangeRequested;
        window.SizeChangeRequested += HandleSizeChangeRequested;
        window.ContentChangeRequested += HandleContentChangeRequested;
        activeWindows.Add(window);
        activeClingies.Add(clingy);
        window.Show();
    }

    public void InitActiveWindows()
    {
        foreach (var clingy in noteService.GetAllActive())
        {
            var window = new ClingyWindow(clingy);
            activeClingies.Add(clingy);
            activeWindows.Add(window);
            window.Show();
        }
    }

    public void RenderAllWindows()
    {
        foreach (var window in activeWindows)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }
    }

    #region EventHandlers
    private void HandleCloseRequested(object? sender, Guid id)
    {
        var window = (ClingyWindow)sender!;
        window.CloseRequested -= HandleCloseRequested;
        activeWindows.Remove(window);
        activeClingies.Remove(activeClingies.Single(x => x.Id == id));
        window.Close();
        noteService.SoftDelete(id);
    }

    private void HandlePinRequested(object? sender, PinRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.PinRequested -= HandlePinRequested;
        window.Topmost = args.IsPinned;
        clingy.SetPinState(args.IsPinned);
        noteService.Update(clingy);
    }

    private void HandlePositionChangeRequested(object? sender, Clingy clingy)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        window.PositionChangeRequested -= HandlePositionChangeRequested;
        clingy.Move(window.Position.X, window.Position.Y);
        noteService.Update(clingy);
    }

    private void HandleSizeChangeRequested(object? sender, Clingy clingy)
    {
        var window = (ClingyWindow)sender!;
        window.SizeChangeRequested -= HandleSizeChangeRequested;
        clingy.Resize(window.Width, window.Height);
        noteService.Update(clingy);
    }

    private void HandleContentChangeRequested(object? sender, Clingy clingy)
    {
        var window = (ClingyWindow)sender!;
        window.ContentChangeRequested -= HandleContentChangeRequested;
        string content = string.IsNullOrEmpty(window.ContentBox.Text!) ? "" : window.ContentBox.Text!;
        clingy.UpdateContent(content);
        noteService.Update(clingy);
    }

    #endregion
}
