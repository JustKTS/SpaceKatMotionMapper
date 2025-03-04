# SpaceKatMotionMapper

本项目用于 SpaceKat DIY 3D鼠标，可将3D动作转换为键鼠按键组合，扩展用法。

## 介绍视频

[https://www.bilibili.com/video/BV1rs9bYoEXG](https://www.bilibili.com/video/BV1rs9bYoEXG)

## 视频所述之外新特性
1. 添加顶层透明浮窗提示，并可自定义大小及位置（v0.1.6+)
2. 可手动添加程序，识别其位于顶层时自动启动官方映射, 其余程序自动启动本程序映射（v0.1.7+)

## TODO
- [ ] 模式切换的验证
- [x] 后台时顶层透明小窗的动作提示
- [ ] 调整UI, 增加层级，简化布局

## 已知Bug
1. ~~静置一段时间不使用后由于读取异常会断联，需要重新连接，尽快修复~~ 已修复
2. 部分使用新的绿色化技术处理的软件无法通过Win32API获取到可执行文件地址，无法进行分应用配置

## 其他
UI由[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)实现

使用[Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)主题和[Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia)控件库

HidApi.dll来自[https://github.com/libusb/hidapi](https://github.com/libusb/hidapi),使用其BSD许可

HID定义与组织学习自[PySpaceMouse](https://github.com/JakubAndrysek/PySpaceMouse)


