using System;
using System.Collections.Generic;
using System.Linq;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Gtk.Windows;
using Clingies.Gtk.Utils;

namespace Clingies.Gtk.Factories;

public class ClingyWindowFactory(ClingyService clingyService, 
                            UtilsService utilsService,
                            IClingiesLogger loggerService,
                            Func<IContextCommandController, IContextCommandProvider> providerFactory)
{
    private readonly List<ClingyDto> _activeClingies = new List<ClingyDto>();
    private readonly List<ClingyWindow> _activeWindows = new List<ClingyWindow>();
    private readonly ClingyService _srvClingy = clingyService;
    private readonly IClingiesLogger _srvLogger = loggerService;
    private readonly UtilsService _srvUtils = utilsService;

    public void CreateNewWindow()
    {
        try
        {
            var clingy = new ClingyDto();
            clingy.Id = _srvClingy.Create(clingy);
            var window = new ClingyWindow(clingy, _srvClingy, _srvUtils);
            var provider = providerFactory(window);
            window.SetContextCommandProvider(provider);
            SubscribeWindowToEvents(window);
            _activeWindows.Add(window);
            _activeClingies.Add(clingy);
            window.Show();
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error creating new Clingy window");
            throw;
        }
    }

    public void InitActiveWindows()
    {
        try
        {
            foreach (var clingy in _srvClingy.GetAllActive())
            {
                var window = new ClingyWindow(clingy, _srvClingy, _srvUtils);
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
            _srvLogger.Error(ex, "Error loading all active Clingies");
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
        window.UpdateWindowSizeRequested += HandleUpdateWindowSizeRequested;
        window.RollRequested += HandleRollRequested;
        window.LockRequested += HandleLockRequested;
    }

    public void RenderAllWindows()
    {
        try
        {
            foreach (var window in _activeWindows)
            {
                //window.Topmost = true;
                window.Activate();
                //window.Topmost = false;
            }
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error rendering all active Clingies");
            throw;
        }
    }

    #region EventHandlers
    private void HandleCloseRequested(object? sender, int id)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == id);
            var clingy = _activeClingies.Single(x => x.Id == id);
            _activeWindows.Remove(window);
            _activeClingies.Remove(clingy);
            window.Close();
            _srvClingy.SoftDelete(id);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleCloseRequested");
            throw;
        }
    }

    private void HandlePinRequested(object? sender, PinRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            //window.Topmost = args.IsPinned;
            clingy.IsPinned = args.IsPinned;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandlePinRequested");
            throw;
        }
    }

    private void HandleLockRequested(object? sender, LockRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            //window.ClingyTitleBar.IsLocked = args.IsLocked;
            //window.ClingyBody.IsLocked = args.IsLocked;
            clingy.IsLocked = args.IsLocked;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleLockRequested");
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
            //window.Position = new PixelPoint(args.PositionX, args.PositionY);
            clingy.PositionX = args.PositionX;
            clingy.PositionY = args.PositionY;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandlePositionChangeRequested");
            throw;
        }
    }

    private void HandleContentChangeRequested(object? sender, ContentChangeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.Content = string.IsNullOrEmpty(args.Content) ? "" : args.Content;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleContentChangeRequested");
            throw;
        }
    }

    private void HandleTitleChangeRequested(object? sender, TitleChangeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.Title = args.NewTitle;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleTitleChangeRequested");
            throw;
        }
    }

    private void HandleUpdateWindowSizeRequested(object? sender, UpdateWindowSizeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            //window.Width = args.Width;
            //window.Height = args.Height;
            clingy.Width = args.Width;
            clingy.Height = args.Height;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleUpdateWindowSizeRequested");
            throw;
        }
    }

    private void HandleRollRequested(object? sender, RollRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            //window.ClingyBody.IsRolled = args.IsRolled;
            clingy.IsRolled = args.IsRolled;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleRollRequested");
            throw;
        }
    }

    #endregion    
}
