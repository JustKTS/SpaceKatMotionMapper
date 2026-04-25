# SpaceKatMotionMapper

本项目用于 SpaceKat DIY 3D鼠标，可将3D动作转换为键鼠按键组合，扩展用法。支持 Windows 和 Linux (目前只支持Niri窗口管理器) 平台。

*注：Linux平台目前主要为自用，按键模拟基于ydotool工具，需要自行安装它，并且自行处理hid设备权限问题。因此暂时不提供编译版，需要的可以自行下载项目编译。*

## 快捷键预设
希望大家多给快捷键预设贡献配置文件，方便大家使用。

**快捷键预设项目地址**
[https://gitee.com/justkts/space-kat-motion-mapper-meta-key-presets](https://gitee.com/justkts/space-kat-motion-mapper-meta-key-presets)

## 介绍视频

### v0.3.0
[https://www.bilibili.com/video/BV182odBUELX](https://www.bilibili.com/video/BV182odBUELX)

### v0.2.7
[https://www.bilibili.com/video/BV18KVKzSE9Y](https://www.bilibili.com/video/BV18KVKzSE9Y)

### v0.2.x
[https://www.bilibili.com/video/BV1Y3QaYjEKv](https://www.bilibili.com/video/BV1Y3QaYjEKv)

### v0.1.x
[https://www.bilibili.com/video/BV1rs9bYoEXG](https://www.bilibili.com/video/BV1rs9bYoEXG)

## 小版本更新新特性说明
1. 添加顶层透明浮窗提示，并可自定义大小及位置（v0.1.6+)
2. 可手动添加程序，识别其位于顶层时自动启动官方映射, 其余程序自动启动本程序映射（v0.1.7+)
3. 按键定义支持快速添加快捷键组合（v0.2.3+)
4. 按键定义支持延时，键盘宏get√（v0.2.3+)
5. 动作选项卡分组配置，简化设置流程（v0.3.0+)
6. 单动作模式：推动直接触发、单键自动重复、组合键默认触发一次（v0.3.0+)
8. 配置冲突检测与明确提示（v0.3.0+)
9. 支持 MiniE 有线模式，应该兼容后续的mini2等设备，欢迎反馈（v0.3.0+)
10. Linux 初步支持，当前适配 Niri 窗口管理器（v0.3.0+)

## TODO
- [x] 模式切换的验证
- [x] 后台时顶层透明小窗的动作提示
- [x] 调整UI, 增加层级，简化布局
- [ ] Linux 支持更多窗口管理器 (Hyprland, KDE 等)*动力不足，不一定做hhh，欢迎提pr*
- [ ] 单动作模式配置在模式切换时的保留
- [ ] 对Mini2等设备的自动休眠进行适配

*由于我还没有制作mini2, mini2的自动休眠可能会导致软件断联并且不会在唤醒时自动重连，这部分监控暂时没有做，所以有需要的建议先连接有线模式使用。*

## 已知Bug
1. ~~部分使用新的绿色化技术处理的软件无法通过Win32API获取到可执行文件地址，无法进行分应用配置~~ 因管理员权限问题，使用管理员权限启动即可。
2. Linux 平台部分功能仍在完善中(系统托盘、弹窗、最小化支持等)，目前仅部分功能适配 Niri 窗口管理器

## 其他
UI由[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)实现

使用[Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)主题和[Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia)控件库

HidApi.dll来自[https://github.com/libusb/hidapi](https://github.com/libusb/hidapi),使用其BSD许可

HID定义与组织学习自[PySpaceMouse](https://github.com/JakubAndrysek/PySpaceMouse)


