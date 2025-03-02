using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using LanguageExt.Common;
using SpaceKatMotionMapper.Models;
using SpaceKatMotionMapper.Services.Contract;
using Avalonia.Controls;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.ViewModels;
using Win32Helpers;

namespace SpaceKatMotionMapper.Services;

public class OfficialMapperSwitchService(ITopLevelHelper topLevelHelper)
{
    # region 热键注册

    private const int HotKeyEventId = 9876;

    public Result<bool> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, HotKeyCodeEnum hotKey,
        KatButtonEnum katButtonEnum)
    {
        OfficialWareConfigFunctions.UnbindHotKeyToKatButton();
        var modifierKeys = 0;
        if (useCtrl) modifierKeys |= 2;
        if (useAlt) modifierKeys |= 1;
        if (useShift) modifierKeys |= 4;


        var handle = topLevelHelper.GetTopLevel().TryGetPlatformHandle();
        if (handle is null) return new Result<bool>(new Exception("获取窗口句柄失败"));
        // ReSharper disable once InvertIf
        if (handle.Handle is { } nativeHandle)
        {
            HotKeyHelpers.UnregisterHotKeyWrapper(nativeHandle, HotKeyEventId);
            var ret = HotKeyHelpers.RegisterHotKeyWrapper(nativeHandle, HotKeyEventId, modifierKeys, (int)hotKey);
            if (!ret) return new Result<bool>(new Exception("注册热键失败"));
            if (katButtonEnum != KatButtonEnum.None)
            {
                OfficialWareConfigFunctions.BindHotKeyToKatButton(katButtonEnum, useCtrl, useAlt, useShift, hotKey);
            }

            return true;
        }

        return false;
    }

    public Result<bool> UnregisterHotKeyWrapper()
    {
        var handle = topLevelHelper.GetTopLevel().TryGetPlatformHandle();
        if (handle is null) return new Result<bool>(new Exception("获取窗口句柄失败"));
        // ReSharper disable once InvertIf
        if (handle.Handle is { } nativeHandle)
        {
            var ret = HotKeyHelpers.UnregisterHotKeyWrapper(nativeHandle, HotKeyEventId);
            return ret;
        }

        return false;
    }

    private static IntPtr HwndHook(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmHotkey = 0x0312;

        switch (msg)
        {
            case wmHotkey:
                switch (wParam.ToInt32())
                {
                    case HotKeyEventId:
                        var vm = App.GetRequiredService<ListeningInfoViewModel>();
                        vm.IsOfficialMapperOff = !vm.IsOfficialMapperOff;
                        break;
                }

                break;
        }

        return IntPtr.Zero;
    }

    public void RegisterHandle()
    {
        Win32Properties.AddWndProcHookCallback(topLevelHelper.GetTopLevel(), HwndHook);
    }

    public void UnregisterHandle()
    {
        Win32Properties.RemoveWndProcHookCallback(topLevelHelper.GetTopLevel(), HwndHook);
    }

    #endregion
}