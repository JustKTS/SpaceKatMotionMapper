﻿using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using SpaceKatMotionMapper.Models;
using Avalonia.Controls;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Functions;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using Win32Helpers;
using WindowsInput;

namespace SpaceKatMotionMapper.Services;

public class OfficialMapperHotKeyService : IOfficialMapperHotKeyService
{
    # region 热键注册

    private const int HotKeyEventId = 9876;

    public async Task<Result<bool>> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, VirtualKeyCode hotKey,
        KatButtonEnum katButtonEnum)
    {
        await OfficialWareConfigFunctions.UnbindHotKeyToKatButton();
        var modifierKeys = 0;
        if (useCtrl) modifierKeys |= 2;
        if (useAlt) modifierKeys |= 1;
        if (useShift) modifierKeys |= 4;


        var handle = TopLevelHelper.GetTopLevel().TryGetPlatformHandle();
        if (handle is null) return new Result<bool>(new Exception("获取窗口句柄失败"));
        // ReSharper disable once InvertIf
        if (handle.Handle is { } nativeHandle)
        {
            HotKeyHelpers.UnregisterHotKeyWrapper(nativeHandle, HotKeyEventId);
            var ret = HotKeyHelpers.RegisterHotKeyWrapper(nativeHandle, HotKeyEventId, modifierKeys, (int)hotKey);
            if (!ret) return new Result<bool>(new Exception("注册热键失败"));
            if (katButtonEnum != KatButtonEnum.None)
            {
                 return await OfficialWareConfigFunctions.BindHotKeyToKatButton(katButtonEnum, useCtrl, useAlt, useShift, hotKey);
            }
            return true;
        }

        return false;
    }

    public Result<bool> UnregisterHotKeyWrapper()
    {
        var handle = TopLevelHelper.GetTopLevel().TryGetPlatformHandle();
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
                        var globalStates = App.GetRequiredService<GlobalStates>();
                        globalStates.IsMapperEnable = !globalStates.IsMapperEnable;
                        break;
                }

                break;
        }

        return IntPtr.Zero;
    }

    public void RegisterHandle()
    {
        Win32Properties.AddWndProcHookCallback(TopLevelHelper.GetTopLevel(), HwndHook);
    }

    public void UnregisterHandle()
    {
        Win32Properties.RemoveWndProcHookCallback(TopLevelHelper.GetTopLevel(), HwndHook);
    }

    #endregion
}