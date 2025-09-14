using System;
using System.Collections.Generic;
using System.Linq;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Interfaces;
using Clingies.Domain.DTOs;
using Clingies.GtkFront.Windows;

namespace Clingies.GtkFront.Services;

public class ClingyWindowManager(ClingyService clingyService,
                            GtkUtilsService utilsService,
                            MenuFactory menuFactory,
                            IClingiesLogger loggerService,
                            ITitleDialogService titleDialogService,
                            Func<IContextCommandController, IContextCommandProvider> providerFactory)
{
    private readonly List<ClingyDto> _activeClingies = new List<ClingyDto>();
    private readonly List<ClingyWindow> _activeWindows = new List<ClingyWindow>();
    private readonly ClingyService _srvClingy = clingyService;
    private readonly IClingiesLogger _srvLogger = loggerService;
    private readonly ITitleDialogService _titleDialogService = titleDialogService;
    private readonly GtkUtilsService _srvUtils = utilsService;
    private readonly MenuFactory _menuFactory = menuFactory;

    public void CreateNewWindow()
    {
        try
        {
            var centerPoint = _srvUtils.GetCenterPointDefaultMonitor(AppConstants.Dimensions.DefaultClingyWidth, AppConstants.Dimensions.DefaultClingyHeight);
            var clingy = new ClingyDto();
            // default values
            clingy.PositionX = centerPoint.X;
            clingy.PositionY = centerPoint.Y;
            clingy.Type = Enums.ClingyType.Desktop;
            clingy.Id = _srvClingy.Create(clingy);

            var controller = new ClingyContextController(this, _titleDialogService, clingy.Id);
            var provider = providerFactory(controller);
            var window = new ClingyWindow(clingy, _srvUtils, _menuFactory, controller);

            window.Move(centerPoint.X, centerPoint.Y);

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
                var controller = new ClingyContextController(this, _titleDialogService, clingy.Id);
                var provider = providerFactory(controller);
                var window = new ClingyWindow(clingy, _srvUtils, _menuFactory, controller);
                window.SetContextCommandProvider(provider);
                SubscribeWindowToEvents(window);
                window.KeepAbove = clingy.IsPinned;
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
                window.KeepAbove = true;
                window.Activate();
                window.KeepAbove = false;
            }
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error rendering all active Clingies");
            throw;
        }
    }

    public ClingyDto? GetClingyDtoById(int clingyId)
    {
        return _activeClingies.Where(x => x.Id == clingyId).FirstOrDefault();
    }

    public ClingyWindow? GetClingyWindowById(int clingyId)
    {
        return _activeWindows.Where(x => x.ClingyId == clingyId).FirstOrDefault();
    }

    #region EventHandlers
    private void HandleCloseRequested(object? sender, int id)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == id);
            var clingy = _activeClingies.Single(x => x.Id == id);
            if (clingy.IsLocked) return;
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
            if (clingy.IsLocked) return;
            window.KeepAbove = args.IsPinned;
            clingy.IsPinned = args.IsPinned;
            window.SetPinIcon(clingy.IsPinned);
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandlePinRequested");
            throw;
        }
    }

    public void RequestLock(int clingyId)
    {
        HandleLockRequested(sender: this, args: new LockRequestedEventArgs(clingyId, true));
    }

    public void RequestUnlock(int clingyId)
    {
        HandleLockRequested(sender: this, args: new LockRequestedEventArgs(clingyId, false));
    }

    private void HandleLockRequested(object? sender, LockRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.IsLocked = args.IsLocked;
            window.SetLockIcon(clingy.IsLocked);
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
            if (clingy.IsLocked) return;
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
            // TODO - HANDLE IMGS
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            if (clingy.IsLocked) return;
            clingy.Text = string.IsNullOrEmpty(args.Content) ? "" : args.Content;
            _srvClingy.Update(clingy);
        }
        catch (Exception ex)
        {
            _srvLogger.Error(ex, "Error at HandleContentChangeRequested");
            throw;
        }
    }

    // Exposing a façade for the private event so we can call it from ContextController or elsewhere,
    //  do the same for others
    public void RequestTitleChange(int clingyId, string newTitle)
    {
        HandleTitleChangeRequested(sender: this, args: new TitleChangeRequestedEventArgs(clingyId, newTitle));
    }

    private void HandleTitleChangeRequested(object? sender, TitleChangeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            clingy.Title = args.NewTitle;
            window.ChangeTitleBarText(args.NewTitle);
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
            if (clingy.IsLocked) return;
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
            if (clingy.IsLocked) return;
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
