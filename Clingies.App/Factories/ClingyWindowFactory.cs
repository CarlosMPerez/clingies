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
        SubscribeWindowToEvents(window);
        activeWindows.Add(window);
        activeClingies.Add(clingy);
        window.Show();
    }

    public void InitActiveWindows()
    {
        foreach (var clingy in noteService.GetAllActive())
        {
            var window = new ClingyWindow(clingy);
            SubscribeWindowToEvents(window);
            activeClingies.Add(clingy);
            activeWindows.Add(window);
            window.Show();
        }
    }

    private void SubscribeWindowToEvents(ClingyWindow window)
    {
        window.CloseRequested += HandleCloseRequested;
        window.PinRequested += HandlePinRequested;
        window.PositionChangeRequested += HandlePositionChangeRequested;
        window.SizeChangeRequested += HandleSizeChangeRequested;
        window.ContentChangeRequested += HandleContentChangeRequested;
        window.TitleChangeRequested += HandleTitleChangeRequested;
        window.UpdateWindowHeightRequested += HandleUpdateWindowHeightRequested;
        window.UpdateWindowWidthRequested += HandleUpdateWindowWidthRequested;
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
        var window = activeWindows.Single(x => x.ClingyId == id);
        var clingy = activeClingies.Single(x => x.Id == id);
        activeWindows.Remove(window);
        activeClingies.Remove(clingy);
        window.Close();
        noteService.SoftDelete(id);
    }

    private void HandlePinRequested(object? sender, PinRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.Topmost = args.IsPinned;
        clingy.SetPinState(args.IsPinned);
        noteService.Update(clingy);
    }

    private void HandlePositionChangeRequested(object? sender,
                        PositionChangeRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.Position = new Avalonia.PixelPoint(args.PositionX, args.PositionY);
        clingy.Move(args.PositionX, args.PositionY);
        noteService.Update(clingy);
    }

    private void HandleSizeChangeRequested(object? sender, SizeChangeRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.Width = args.Width;
        window.Height = args.Height;
        clingy.Resize(args.Width, args.Height);
        noteService.Update(clingy);
    }

    private void HandleContentChangeRequested(object? sender, ContentChangeRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.ContentBox.Text = args.Content;
        clingy.UpdateContent(args.Content);
        noteService.Update(clingy);
    }

    private async void HandleTitleChangeRequested(object? sender, TitleChangeRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        var dialog = new TitleDialog(args.PreviousTitle);

        var result = await dialog.ShowDialog<string>(window);
        if (!string.IsNullOrWhiteSpace(result))
        {
            clingy.UpdateTitle(result);
            noteService.Update(clingy);
            window.TitleTextBlock.Text = result;
        }
    }

    private void HandleUpdateWindowHeightRequested(object? sender, UpdateWindowHeightRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.Height = args.Height;
        clingy.Resize(window.Width, args.Height);
        noteService.Update(clingy);
    }

    private void HandleUpdateWindowWidthRequested(object? sender, UpdateWindowWidthRequestedEventArgs args)
    {
        var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
        var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
        window.Width = args.Width;
        clingy.Resize(args.Width, window.Height);
        noteService.Update(clingy);
    }

    #endregion
}
