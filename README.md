# SpaceKatMotionMapper

本项目用于 SpaceKat DIY 3D鼠标，可将3D动作转换为键鼠按键组合，扩展用法。

## 介绍视频

[https://www.bilibili.com/video/BV1rs9bYoEXG](https://www.bilibili.com/video/BV1rs9bYoEXG)

## TODO
- [ ] 模式切换的验证
- [ ] 后台时顶层透明小窗的动作提示

## 已知Bug
1. 静置一段时间不使用后由于读取异常会断联，需要重新连接，尽快修复
2. 部分使用新的绿色化技术处理的软件无法通过Win32API获取到可执行文件地址，无法进行分应用配置

## 其他
UI由[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)实现

使用[Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)主题和[Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia)控件库

HidApi.dll来自[https://github.com/libusb/hidapi](https://github.com/libusb/hidapi),使用其BSD许可

HID定义与组织学习自[PySpaceMouse](https://github.com/JakubAndrysek/PySpaceMouse)


