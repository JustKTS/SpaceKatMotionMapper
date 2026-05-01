using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CSharpFunctionalExtensions;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.Models;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Services.Contract;
using SpaceKatMotionMapper.States;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.Services;

public class OfficialMapperHotKeyService : IOfficialMapperHotKeyService
{
    private readonly IPlatformHotKeyService _hotKeyService;

    public OfficialMapperHotKeyService(IPlatformHotKeyService hotKeyService)
    {
        _hotKeyService = hotKeyService;
    }

    # region 热键注册

    private const int HotKeyEventId = 9876;

    public async Task<Result<bool, Exception>> RegisterHotKeyWrapper(bool useCtrl, bool useAlt, bool useShift, KeyCodeWrapper hotKey,
        KatButtonEnum katButtonEnum)
    {
        await OfficialWareConfigFunctions.UnbindHotKeyToKatButton();
        var modifierKeys = 0;
        if (useCtrl) modifierKeys |= 2;
        if (useAlt) modifierKeys |= 1;
        if (useShift) modifierKeys |= 4;


        var handle = TopLevelHelper.GetTopLevel().TryGetPlatformHandle();
        if (handle is null) return new Exception("获取窗口句柄失败");
        // ReSharper disable once InvertIf
        if (handle.Handle is { } nativeHandle)
        {
            if (!_hotKeyService.IsSupported)
            {
                return new Exception("热键功能仅支持Windows平台");
            }

            _hotKeyService.UnregisterHotKeyWrapper(nativeHandle, HotKeyEventId);
            var ret = _hotKeyService.RegisterHotKeyWrapper(nativeHandle, HotKeyEventId, modifierKeys, (int)hotKey);
            if (!ret) return new Exception("注册热键失败");
            if (katButtonEnum != KatButtonEnum.None)
            {
                 return await OfficialWareConfigFunctions.BindHotKeyToKatButton(katButtonEnum, useCtrl, useAlt, useShift, hotKey);
            }
            return true;
        }

        // ReSharper disable once HeuristicUnreachableCode
        return false;
    }

    public Result<bool, Exception> UnregisterHotKeyWrapper()
    {
        var handle = TopLevelHelper.GetTopLevel().TryGetPlatformHandle();
        if (handle is null) return new Exception("获取窗口句柄失败");
        // ReSharper disable once InvertIf
        if (handle.Handle is { } nativeHandle)
        {
            var ret = _hotKeyService.UnregisterHotKeyWrapper(nativeHandle, HotKeyEventId);
            return ret;
        }

        // ReSharper disable once HeuristicUnreachableCode
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