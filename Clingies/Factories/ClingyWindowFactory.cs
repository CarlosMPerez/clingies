using System;
using System.Linq;
using System.Collections.Generic;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Models;
using Clingies.Windows;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.Domain.Interfaces;
using Clingies.ApplicationLogic.Interfaces;
using Avalonia.Media;
using Clingies.UserControls;
using Avalonia.Controls;

namespace Clingies.Factories;

public class ClingyWindowFactory(ClingyService noteService,
                                StyleService styleService,
                                IClingiesLogger logger,
                                Func<IContextCommandController, IContextCommandProvider> providerFactory)
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
            ApplyStyle(clingy, window);
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
                ApplyStyle(clingy, window);
                window.Show();
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error loading all active Clingies");
            throw;
        }
    }

    private void ApplyStyle(Clingy clingy, ClingyWindow window)
    {
        Style style = clingy.Style != null ? styleService.Get(clingy.Style) : styleService.GetDefault();
        Console.WriteLine("Applying style {0}", style.Id);
        window.ClingyBody.ClearValue(Panel.BackgroundProperty);
        window.ClingyBody.Background = Brush.Parse(style.BodyColor);
        window.ClingyBody.ContentBox.ClearValue(TextBox.FontFamilyProperty);
        window.ClingyBody.ContentBox.FontFamily = new FontFamily(style.BodyFont);
        window.ClingyBody.ContentBox.ClearValue(TextBox.FontSizeProperty);
        window.ClingyBody.ContentBox.FontSize = (double)style.BodyFontSize;
        window.ClingyBody.ContentBox.ClearValue(TextBox.ForegroundProperty);
        window.ClingyBody.ContentBox.Foreground = Brush.Parse(style.BodyFontColor);
        // TODO Text decorations

        window.ClingyTitleBar.ClearValue(Panel.BackgroundProperty);
        window.ClingyTitleBar.Background = Brush.Parse(style.TitleColor);
        window.ClingyTitleBar.TitleText.ClearValue(TextBox.FontFamilyProperty);
        window.ClingyTitleBar.FontFamily = new FontFamily(style.TitleFont);
        window.ClingyTitleBar.TitleText.ClearValue(TextBox.FontSizeProperty);
        window.ClingyTitleBar.FontSize = (double)style.TitleFontSize;
        window.ClingyTitleBar.TitleText.ClearValue(TextBox.ForegroundProperty);
        window.ClingyTitleBar.TitleText.Foreground = Brush.Parse(style.TitleFontColor);
        // TODO Text decorations
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
    private void HandleCloseRequested(object? sender, int id)
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
            clingy.IsPinned = args.IsPinned;
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
            clingy.IsLocked = args.IsLocked;
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
            clingy.PositionX = args.PositionX;
            clingy.PositionY = args.PositionY;
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
            clingy.Content = string.IsNullOrEmpty(args.Content) ? "" : args.Content;
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
            clingy.Title = args.NewTitle;
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleTitleChangeRequested");
            throw;
        }
    }

    private void HandleUpdateWindowSizeRequested(object? sender, UpdateWindowSizeRequestedEventArgs args)
    {
        try
        {
            var window = _activeWindows.Single(x => x.ClingyId == args.ClingyId);
            var clingy = _activeClingies.Single(x => x.Id == args.ClingyId);
            window.Width = args.Width;
            window.Height = args.Height;
            clingy.Width = args.Width;
            clingy.Height = args.Height;
            noteService.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at HandleUpdateWindowSizeRequested");
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
            clingy.IsRolled = args.IsRolled;
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
