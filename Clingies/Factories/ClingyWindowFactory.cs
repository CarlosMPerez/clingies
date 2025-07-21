using System;
using System.Linq;
using System.Collections.Generic;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Models;
using Clingies.Windows;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.Domain.Interfaces;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Services;
using Avalonia.Media;

namespace Clingies.Factories;

public class ClingyWindowFactory(ClingyService noteService,
                                IClingiesLogger logger,
                                Func<IContextCommandController, IContextCommandProvider> providerFactory,
                                UtilsService utils)
{
    private readonly List<Clingy> _activeClingies = new List<Clingy>();
    private readonly List<ClingyWindow> _activeWindows = new List<ClingyWindow>();
    public void CreateNewWindow(double posX, double posY)
    {
        try
        {
            var clingy = noteService.Create("", "", posX, posY);
            var window = new ClingyWindow(clingy);
            var provider = providerFactory(window);
            window.SetContextCommandProvider(provider);
            SubscribeWindowToEvents(window);
            _activeWindows.Add(window);
            _activeClingies.Add(clingy);
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
                var provider = providerFactory(window);
                window.SetContextCommandProvider(provider);
                SubscribeWindowToEvents(window);
                _activeClingies.Add(clingy);
                _activeWindows.Add(window);
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
        window.ContentChangeRequested += HandleContentChangeRequested;
        window.TitleChangeRequested += HandleTitleChangeRequested;
        window.UpdateWindowWidthRequested += HandleUpdateWindowWidthRequested;
        window.RollRequested += HandleRollRequested;
        window.LockRequested += HandleLockRequested;
    }

    public void RenderAllWindows()
    {
        try
        {
            foreach (var window in _activeWindows)
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
            var window = _activeWindows.Single(x => x.ClingyId == id);
            var clingy = _activeClingies.Single(x => x.Id == id);
            _activeWindows.Remove(window);
            _activeClingies.Remove(clingy);
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
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.Topmost = args.IsPinned;
            clingy.SetPinState(args.IsPinned);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandlePinRequested");
            throw;
        }
    }

    private void HandleLockRequested(object? sender, LockRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.ClingyTitleBar.IsLocked = args.IsLocked;
            window.ClingyBody.IsLocked = args.IsLocked;
            clingy.SetLockState(args.IsLocked);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleLockRequested");
            throw;
        }
    }

    private void HandlePositionChangeRequested(object? sender,
                        PositionChangeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.Position = new Avalonia.PixelPoint(args.PositionX, args.PositionY);
            clingy.Move(args.PositionX, args.PositionY);
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
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.UpdateContent(string.IsNullOrEmpty(args.Content) ? "" : args.Content);
            clingy.Resize(clingy.Width, args.Height);
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleContentChangeRequested");
            throw;
        }
    }

    private void HandleTitleChangeRequested(object? sender, TitleChangeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.UpdateTitle(args.NewTitle);
            noteService.Update(clingy);
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
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.Height = args.Height;
            clingy.Resize(clingy.Width, args.Height);
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
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.Width = args.Width;
            clingy.Resize(args.Width, window.Height);
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
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.ClingyBody.IsRolled = args.IsRolled;
            clingy.SetRolledState(args.IsRolled);
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
