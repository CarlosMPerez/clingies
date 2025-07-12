using System.Collections.Generic;
using Clingies.Application.Services;
using Clingies.Domain.Models;
using Clingies.App.Windows;
using System;
using Clingies.App.Common.CustomEventArgs;
using System.Linq;
using Clingies.Common;

namespace Clingies.App.Factories;

public class ClingyWindowFactory(ClingyService noteService, IClingiesLogger logger)
{
    private List<Clingy> activeClingies = new List<Clingy>();
    private List<ClingyWindow> activeWindows = new List<ClingyWindow>();
    public void CreateNewWindow(double posX, double posY)
    {
        try
        {
            var clingy = noteService.Create("", "", posX, posY);
            var window = new ClingyWindow(clingy);
            SubscribeWindowToEvents(window);
            activeWindows.Add(window);
            activeClingies.Add(clingy);
            window.Show();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating new Clingy window");
            throw;
        }
    }

    public void InitActiveWindows()
    {
        try
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
        catch (Exception ex)
        {
            logger.Error(ex, "Error loading all active Clingies");
            throw;
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
        window.UpdateWindowWidthRequested += HandleUpdateWindowWidthRequested;
        window.RollRequested += HandleRollRequested;
    }

    public void RenderAllWindows()
    {
        try
        {
            foreach (var window in activeWindows)
            {
                window.Topmost = true;
                window.Activate();
                window.Topmost = false;
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error rendering all active Clingies");
            throw;
        }
    }

    #region EventHandlers
    private void HandleCloseRequested(object? sender, Guid id)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == id);
            var clingy = activeClingies.Single(x => x.Id == id);
            activeWindows.Remove(window);
            activeClingies.Remove(clingy);
            window.Close();
            noteService.SoftDelete(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleCloseRequested");
            throw;
        }
    }

    private void HandlePinRequested(object? sender, PinRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            window.Topmost = args.IsPinned;
            clingy.SetPinState(args.IsPinned);
            logger.Info("{0} has requested a pin", clingy.Title!);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandlePinRequested");
            throw;
        }
    }

    private void HandlePositionChangeRequested(object? sender,
                        PositionChangeRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            window.Position = new Avalonia.PixelPoint(args.PositionX, args.PositionY);
            clingy.Move(args.PositionX, args.PositionY);
            logger.Info("Position changed requested {0}. New Position XY: {1}-{2}",
                    clingy.Title!, args.PositionX, args.PositionY);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandlePositionChangeRequested");
            throw;
        }
    }

    private void HandleContentChangeRequested(object? sender, ContentChangeRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            window.ContentBox.Text = args.Content;
            clingy.UpdateContent(args.Content);
            logger.Info("{0} has requested a change in content", clingy.Title!);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleContentChangeRequested");
            throw;
        }
    }

    private async void HandleTitleChangeRequested(object? sender, TitleChangeRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            var dialog = new TitleDialog(args.PreviousTitle);

            var result = await dialog.ShowDialog<string>(window);
            if (!string.IsNullOrWhiteSpace(result))
            {
                clingy.UpdateTitle(result);
                logger.Info("{0} has requested a title change", clingy.Title!);
                noteService.Update(clingy);
                window.TitleTextBlock.Text = result;
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleTitleChangeRequested");
            throw;
        }
    }

    private void HandleSizeChangeRequested(object? sender, SizeChangeRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            window.Height = args.Height;
            clingy.Resize(clingy.Width, args.Height);
            logger.Info("New size requested {0}. Width/Height: {1}/{2}",
                        clingy.Title!, clingy.Width, args.Height);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleSizeChangeRequested");
            throw;
        }
    }

    private void HandleUpdateWindowWidthRequested(object? sender, UpdateWindowWidthRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            window.Width = args.Width;
            clingy.Resize(args.Width, window.Height);
            logger.Info("New width requested {0}. Width/Height: {1}/{2}",
                    clingy.Title!, args.Width, window.Height);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleUpdateWindowWidthRequested");
            throw;
        }
    }

    private void HandleRollRequested(object? sender, RollRequestedEventArgs args)
    {
        try
        {
            var window = activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.SetRolledState(args.IsRolled);
            logger.Info("Change Rolled status requested {0}. IsRolled: {1}",
                    clingy.Title!, args.IsRolled);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleRollRequested");
            throw;
        }
    }

    #endregion
}
