using System;
using System.Collections.Generic;
using System.Linq;
using Clingies.Application.Interfaces;
using Clingies.Application.Services;
using Clingies.Domain.Models;
using Clingies.Windows;

namespace Clingies.Services;

public class ClingyWindowManager(ClingyService clingyService,
                            GtkUtilsService utilsService,
                            StyleService styleService,
                            AppMenuFactory menuFactory,
                            IClingiesLogger loggerService,
                            ITitleDialogService titleDialogService)
{
    private readonly List<ClingyModel> _activeClingies = new List<ClingyModel>();
    private readonly List<ClingyWindow> _activeWindows = new List<ClingyWindow>();
    private readonly ClingyStylingService _stylingService = new(styleService);

    public void CreateNewWindow()
    {
        try
        {
            var centerPoint = utilsService.GetCenterPointDefaultMonitor(AppConstants.Dimensions.DefaultClingyWidth, AppConstants.Dimensions.DefaultClingyHeight);
            var clingy = new ClingyModel
            {
                // default values
                StyleId = styleService.GetDefault().Id,
                PositionX = centerPoint.X,
                PositionY = centerPoint.Y,
                Type = Enums.ClingyType.Desktop
            };
            clingy.Id = clingyService.Create(clingy);

            var controller = new ClingyContextController(this, _stylingService, titleDialogService, clingy.Id);
            var window = new ClingyWindow(clingy, utilsService, menuFactory, controller);
            // Styling
            window.StyleContext.AddClass("clingy");
            _stylingService.ApplyTo(window, clingy.Id, clingy.StyleId);

            window.Move(centerPoint.X, centerPoint.Y);

            SubscribeWindowToEvents(window);
            _activeWindows.Add(window);
            _activeClingies.Add(clingy);
            window.Show();
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error creating new Clingy window");
            throw;
        }
    }

    public void InitActiveWindows()
    {
        try
        {
            foreach (var clingy in clingyService.GetAllActive())
            {
                var controller = new ClingyContextController(this, _stylingService, titleDialogService, clingy.Id);
                var window = new ClingyWindow(clingy, utilsService, menuFactory, controller);
                // Styling
                window.StyleContext.AddClass("clingy");
                _stylingService.ApplyTo(window, clingy.Id, clingy.StyleId);
                SubscribeWindowToEvents(window);
                window.KeepAbove = clingy.IsPinned;
                _activeClingies.Add(clingy);
                _activeWindows.Add(window);
                window.Show();
            }
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error loading all active Clingies");
            throw;
        }
    }
    public void ShowStyleManagerDialog()
    {
        var dialog = new StyleManagerDialog(styleService, loggerService);
        dialog.Show();
    }

    public void RequestLock(int clingyId) =>
        HandleLockRequested(clingyId, true);

    public void RequestUnlock(int clingyId) =>
        HandleLockRequested(clingyId, false);

    public void RequestRollUp(int clingyId) =>
        HandleRollRequested(clingyId, true);

    public void RequestRollDown(int clingyId) =>
        HandleRollRequested(clingyId, false);

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
            loggerService.Error(ex, "Error rendering all active Clingies");
            throw;
        }
    }

    public ClingyModel? GetClingyModelById(int clingyId) =>
        _activeClingies.Where(x => x.Id == clingyId).FirstOrDefault();

    public ClingyWindow? GetClingyWindowById(int clingyId) =>
        _activeWindows.Where(x => x.ClingyId == clingyId).FirstOrDefault();

    public void RequestTitleChange(int clingyId, string newTitle) =>
        HandleTitleChangeRequested(clingyId, newTitle);

    public void RequestStyleChange(int clingyId, int styleId) =>
        HandleStyleChangeRequested(clingyId, styleId);

    private void SubscribeWindowToEvents(ClingyWindow window)
    {
        window.CloseRequested += HandleCloseRequested;
        window.PinRequested += HandlePinRequested;
        window.PositionChangeRequested += HandlePositionChangeRequested;
        window.ContentChangeRequested += HandleContentChangeRequested;
        window.TitleChangeRequested += HandleTitleChangeRequested;
        window.UpdateWindowSizeRequested += HandleUpdateWindowSizeRequested;
        window.RollRequested += HandleRollRequested;
    }

    private void HandleCloseRequested(int id)
    {
        try
        {
            var window = _activeWindows.FirstOrDefault(x => x.ClingyId == id);
            var clingy = _activeClingies.FirstOrDefault(x => x.Id == id);
            if (window is null || clingy is null) return;
            if (clingy.IsLocked) return;
            window.BeginClose();
            _activeWindows.Remove(window);
            _activeClingies.Remove(clingy);
            window.Close();
            clingyService.SoftDelete(id);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleCloseRequested");
            throw;
        }
    }

    private void HandlePinRequested(int clingyId, bool isPinned)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == clingyId);
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            if (clingy.IsLocked) return;
            window.KeepAbove = isPinned;
            clingy.IsPinned = isPinned;
            window.SetPinIcon(clingy.IsPinned);
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandlePinRequested");
            throw;
        }
    }

    private void HandleLockRequested(int clingyId, bool isLocked)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == clingyId);
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            clingy.IsLocked = isLocked;
            window.ApplyLockState(clingy.IsLocked);
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleLockRequested");
            throw;
        }
    }

    private void HandlePositionChangeRequested(int clingyId, int positionX, int positionY)
    {
        try
        {
            var clingy = _activeClingies.FirstOrDefault(x => x.Id == clingyId);
            if (clingy is null) return;
            if (clingy.IsLocked) return;
            clingy.PositionX = positionX;
            clingy.PositionY = positionY;
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandlePositionChangeRequested");
            throw;
        }
    }

    private void HandleContentChangeRequested(int clingyId, string? content)
    {
        try
        {
            // TODO - HANDLE IMGS
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            if (clingy.IsLocked) return;
            clingy.Text = string.IsNullOrEmpty(content) ? "" : content;
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleContentChangeRequested");
            throw;
        }
    }

    private void HandleTitleChangeRequested(int clingyId, string newTitle)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == clingyId);
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            if (clingy.IsLocked) return;
            clingy.Title = newTitle;
            window.ChangeTitleBarText(newTitle);
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleTitleChangeRequested");
            throw;
        }
    }

    private void HandleUpdateWindowSizeRequested(int clingyId, double width, double height)
    {
        try
        {
            var clingy = _activeClingies.FirstOrDefault(x => x.Id == clingyId);
            if (clingy is null) return;
            if (clingy.IsLocked) return;
            clingy.Width = width;
            clingy.Height = height;
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleUpdateWindowSizeRequested");
            throw;
        }
    }

    private void HandleStyleChangeRequested(int clingyId, int styleId)
    {
        try
        {
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            if (clingy.IsLocked) return;
            clingy.StyleId = styleId;
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleRollRequested");
            throw;
        }
    }


    private void HandleRollRequested(int clingyId, bool isRolled)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == clingyId);
            var clingy = _activeClingies.Single(x => x.Id == clingyId);
            if (clingy.IsLocked) return;
            clingy.IsRolled = isRolled;
            window.ApplyRollState(isRolled);
            clingyService.Update(clingy);
        }
        catch (Exception ex)
        {
            loggerService.Error(ex, "Error at HandleRollRequested");
            throw;
        }
    }
}
