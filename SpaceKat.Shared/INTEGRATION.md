# SpaceKat.Shared 键动作编辑库 — 集成教程

本教程将带你从零开始，了解 SpaceKat.Shared 提供了什么，以及如何在一个新项目中使用它来构建键动作编辑功能。

---

## 这个库解决什么问题？

你需要在多个项目中复用同一套「键动作编辑」能力——用户可以添加键盘按键、鼠标点击、滚轮滚动、延时等动作，组合成一个动作序列。但不同项目的业务规则不同：

- 项目 A 的 PressMode 有 Press/Click/Release 三种，项目 B 只允许 Press
- 项目 A 的滚轮倍率可以为负，项目 B 要求必须为正
- 项目 A 需要组合键展开（Ctrl+Shift+A → 一串按键），项目 B 不需要

SpaceKat.Shared 把「可复用的编辑逻辑」和「项目特有的业务规则」分离开来。你只需要提供规则，编辑器开箱即用。

---

## 教程 1：最简单的接入——什么都不用配置

### 目标

在你的项目中显示一个键动作编辑器，用户可以增删动作，读写配置。

### 步骤

**1. 添加项目引用**

```xml
<!-- 你的项目.csproj -->
<ProjectReference Include="..\SpaceKat.Shared\SpaceKat.Shared.csproj" />
```

**2. 在 ViewModel 中创建编辑器**

```csharp
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Models;

public class MyPageViewModel
{
    // 这就是编辑器。默认配置开箱即用，不需要传任何参数。
    public KeyActionConfigV2ViewModel Editor { get; } = new();

    // 保存：获取用户编辑好的动作列表
    public List<KeyActionConfig> SaveActions()
    {
        return Editor.ToKeyActionConfigList();
    }

    // 加载：从文件/网络读取配置并回填到编辑器
    public void LoadActions(List<KeyActionConfig> savedConfigs)
    {
        Editor.FromKeyActionConfig(savedConfigs);
    }
}
```

**3. 在 AXAML 中绑定控件**

```xml
<!-- 你的页面.axaml -->
<!-- 先添加命名空间 -->
xmlns:shared="clr-namespace:SpaceKat.Shared.Views;assembly=SpaceKat.Shared"

<!-- 放控件，绑定到刚才创建的 Editor -->
<shared:KeyActionConfigV2Control DataContext="{Binding Editor}" />
```

**4. 运行**

此时你已经拥有一个完整的键动作编辑器。用户可以：
- 切换动作类型（键盘/鼠标/延时）
- 选择按键
- 设置 PressMode（Press/Click/Release/DoubleClick）
- 设置滚轮倍率或延时毫秒
- 添加、插入、删除动作
- 使用组合键快捷输入（Ctrl+Shift+A 等）

编辑器的 `IsAvailable` 属性会自动反映当前配置是否合法。

### 发生了什么？

`KeyActionConfigV2ViewModel` 内部使用了 `DefaultSharedKeyActionConfigStrategyProfile`，它提供了所有默认行为：

| 策略 | 默认行为 |
|------|---------|
| PressMode 策略 | ActionType 切换时 PressMode 归零，由用户手动选择 |
| 可用性校验 | 键盘需要非空键 + 非 None PressMode，滚轮倍率 != 0，延时 >= 15ms |
| 组合键展开 | Ctrl+Shift+A → Press(Ctrl) + Press(Shift) + Click(A) + Release(Shift) + Release(Ctrl) |

如果这些默认行为符合你的需求，到这里就可以结束了。

---

## 教程 2：自定义策略——你的项目有特殊规则

### 场景

你的项目要求：
1. 键鼠动作只允许 Press 模式（不允许 Click/Release/DoubleClick）
2. 滚轮倍率必须 > 0

### 理解策略装配器

所有策略通过一个叫 `ISharedKeyActionConfigStrategyProfile` 的接口统一装配。把它想象成一个「配置包」——编辑器从里面取出需要的策略。

```
你的配置包 (ISharedKeyActionConfigStrategyProfile)
  ├── PressMode 策略    → 决定 PressMode 的默认值和约束
  ├── 可用性校验器      → 决定配置是否合法
  └── 组合键展开服务    → 决定组合键如何展开为动作序列
```

### 步骤

**1. 创建自定义配置包**

```csharp
using SpaceKat.Shared.Services.Contract;
using SpaceKat.Shared.Services;
using SpaceKat.Shared.Functions;
using SpaceKat.Shared.ViewModels.PressModePolicies;

public class MyProjectProfile : ISharedKeyActionConfigStrategyProfile
{
    // 组合键展开：使用内置实现即可
    public IHotKeyActionExpansionService HotKeyActionExpansionService { get; }
        = new HotKeyActionExpansionService();

    // PressMode 策略：使用内置的「强制 Press」策略
    public IKeyActionPressModePolicy PressModePolicy { get; }
        = new SingleActionKeyActionPressModePolicy();

    // 可用性校验：使用内置实现即可
    public IKeyActionAvailabilityValidator AvailabilityValidator { get; }
        = new KeyActionAvailabilityValidator();

    public IKeyActionPressModePolicy DefaultPressModePolicy => PressModePolicy;
}
```

**2. 注入到编辑器**

```csharp
public class MyPageViewModel
{
    // 传入自定义配置包
    public KeyActionConfigV2ViewModel Editor { get; }

    public MyPageViewModel()
    {
        Editor = new KeyActionConfigV2ViewModel(strategyProfile: new MyProjectProfile());
    }
}
```

现在编辑器会用 `SingleActionKeyActionPressModePolicy`：
- 用户切换到键盘/鼠标类型时，PressMode 自动设为 `Press` 且无法更改
- 其他类型（延时）不受影响

**3. 自定义滚轮校验**

`ISharedKeyActionConfigStrategyProfile` 的 `AvailabilityValidator` 不直接控制滚轮校验。滚轮校验通过 `KeyActionAvailabilityValidationOptions` 控制，需要继承基类来覆盖：

```csharp
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Functions.Contract;

public class MyEditorViewModel : KeyActionConfigEditableBaseViewModel
{
    public MyEditorViewModel()
        : base(new MyProjectProfile()) {}

    // 覆盖此方法：滚轮倍率必须 > 0
    protected override KeyActionAvailabilityValidationOptions GetAvailabilityOptions()
        => new(RequirePositiveScrollMultiplier: true);

    public List<KeyActionConfig> Save() => ToKeyActionConfigListCore();
    public bool Load(IEnumerable<KeyActionConfig> configs)
        => FromKeyActionConfigCore(configs.ToList());
}
```

在 AXAML 中，你仍然可以使用共享的 `KeyActionsItemsControl` 来渲染动作列表：

```xml
<shared:KeyActionsItemsControl DataContext="{Binding Editor.ActionConfigGroups}" />
```

---

## 教程 3：从零构建自定义编辑器

### 场景

教程 1 和 2 使用了内置的 `KeyActionConfigV2ViewModel`。但你的项目可能需要完全不同的编辑器——比如不同的头部按钮、不同的布局。这时你需要继承 `KeyActionConfigEditableBaseViewModel` 来构建。

### 理解基类提供了什么

`KeyActionConfigEditableBaseViewModel` 是一个抽象基类，它管理一个 `ObservableCollection<KeyActionWithCommandViewModel>` 集合。你可以把它理解为一个「动作列表管理器」：

```
KeyActionConfigEditableBaseViewModel 提供：
  ├── ActionConfigGroups          动作集合（ObservableCollection）
  ├── IsAvailable                 所有动作都合法时为 true
  ├── AddActionConfig()           添加一条默认动作
  ├── ToKeyActionConfigListCore() 序列化为 List<KeyActionConfig>
  ├── FromKeyActionConfigCore()   从 List<KeyActionConfig> 反序列化
  └── GetAvailabilityOptions()    可覆盖：返回校验选项
```

### 步骤

```csharp
using SpaceKat.Shared.ViewModels;
using SpaceKat.Shared.Models;
using SpaceKat.Shared.Services.Contract;

public class MyCustomEditor : KeyActionConfigEditableBaseViewModel
{
    public MyCustomEditor(ISharedKeyActionConfigStrategyProfile? profile = null)
        : base(profile) {}

    // --- 公开读写 API ---

    public List<KeyActionConfig> Export()
    {
        return ToKeyActionConfigListCore();
    }

    public bool Import(IEnumerable<KeyActionConfig> configs)
    {
        return FromKeyActionConfigCore(configs.ToList());
    }

    // --- 你自己的业务属性/命令 ---

    public string CustomHeader { get; set; } = "我的动作编辑器";

    // 可以添加你自己的命令，比如「从预设加载」「保存到云端」等
}
```

### 单条动作的结构

集合中的每个元素是 `KeyActionWithCommandViewModel`。了解它有助于你在 AXAML 中正确绑定：

| 属性 | 类型 | 说明 |
|------|------|------|
| `ActionType` | enum | KeyBoard / Mouse / Delay |
| `Key` | string | 按键名，如 "A"、"ScrollUp" |
| `PressMode` | enum | None / Press / Click / Release / DoubleClick |
| `Multiplier` | int | 滚轮倍率或延时毫秒 |
| `IsAvailable` | bool | 当前动作配置是否合法 |
| `IsDelay` | bool | 是否为延时动作（方便 UI 切换模板） |
| `RemoveActionConfigCommand` | ICommand | 删除自身（由基类绑定） |
| `InsertNextActionConfigCommand` | ICommand | 在后方插入一条新动作（由基类绑定） |
| `InsertNextDelayConfigCommand` | ICommand | 在后方插入一条延时（由基类绑定） |

这三个命令在基类的工厂方法 `CreateManagedActionConfigVm()` 中自动绑定。你不需要手动处理。

---

## 教程 4：动态 PressMode——运行时切换模式

### 场景

你的项目有一个开关，用户可以在「高级模式」和「单动作模式」之间切换。在高级模式下 PressMode 有 Press/Click/Release，在单动作模式下只允许 Press。

### 问题

`KeyActionWithCommandViewModel` 的 PressMode 策略在构造时确定。如果直接传入 `DefaultKeyActionPressModePolicy`，后续切换到单动作模式时，ActionType 变化后 PressMode 仍然会被设为 None 而非 Press。

### 解决方案：DelegatingPressModePolicy

使用代理模式，让 PressMode 策略在每次需要时重新求值：

```csharp
using SpaceKat.Shared.ViewModels.PressModePolicies;
using SpaceKat.Shared.Models;

/// <summary>
/// 代理策略：每次调用时从 provider 获取当前策略。
/// 适用于 PressMode 规则在运行时可能变化的场景。
/// </summary>
public class DelegatingPressModePolicy(Func<IKeyActionPressModePolicy> provider)
    : IKeyActionPressModePolicy
{
    public PressModeEnum GetDefaultPressMode(ActionType actionType)
        => provider().GetDefaultPressMode(actionType);

    public PressModeEnum CoercePressMode(ActionType actionType, PressModeEnum pressMode)
        => provider().CoercePressMode(actionType, pressMode);
}
```

在创建动作时注入动态策略：

```csharp
// 在你的编辑器 ViewModel 中
private bool _isSingleActionMode;

private IKeyActionPressModePolicy ResolveCurrentPolicy()
{
    return _isSingleActionMode
        ? new SingleActionKeyActionPressModePolicy()
        : new DefaultKeyActionPressModePolicy();
}

// 创建动作时使用代理
var item = new KeyActionWithCommandViewModel(
    ActionType.KeyBoard, "NONE", PressModeEnum.None, 1,
    pressModePolicy: new DelegatingPressModePolicy(ResolveCurrentPolicy)
);
```

现在当 `_isSingleActionMode` 变化后，用户切换 ActionType 时 PressMode 会自动跟随新模式。

---

## 教程 5：理解组合键展开

### 什么时候需要关注组合键？

当你想让用户快速输入 Ctrl+Shift+A 这样的组合键，并自动展开为一组按键动作时。

### 展开逻辑

`IHotKeyActionExpansionService.Expand()` 根据模式生成不同序列：

**高级模式** (`isSingleActionMode = false`)：
```
Ctrl+Shift+A → [
  Press(Ctrl), Press(Shift), Click(A), Release(Shift), Release(Ctrl)
]
```

**单动作模式** (`isSingleActionMode = true`)：
```
Ctrl+Shift+A → [
  Press(Ctrl+Shift+A)
]
```

### 使用方式

`KeyActionConfigV2ViewModel` 已内置组合键支持。如果你继承 `KeyActionConfigEditableBaseViewModel` 自行构建，可以手动调用：

```csharp
var expansionService = new HotKeyActionExpansionService();
var combinationKeys = new CombinationKeysRecord(
    useCtrl: true, useShift: false, useAlt: false, useWin: false,
    key: new KeyCodeWrapper(/* 你的 KeyCode */)
);
var actions = expansionService.Expand(combinationKeys, isSingleActionMode: false);
// actions 是 List<KeyActionConfig>，可以直接喂给 FromKeyActionConfig
```

---

## 内置的两种 PressMode 策略对比

| 行为 | DefaultKeyActionPressModePolicy | SingleActionKeyActionPressModePolicy |
|------|--------------------------------|-------------------------------------|
| `GetDefaultPressMode(键盘)` | 返回 None（用户手动选） | 返回 Press |
| `GetDefaultPressMode(鼠标)` | 返回 None | 返回 Press |
| `GetDefaultPressMode(延时)` | 返回 None | 返回 None |
| `CoercePressMode(键盘, Click)` | 原样返回 Click | 强制返回 Press |
| `CoercePressMode(延时, 任意)` | 原样返回 | 原样返回 |

---

## 可用性校验规则一览

`KeyActionAvailabilityValidator` 的判定逻辑：

| ActionType | 合法条件 |
|------------|---------|
| KeyBoard | Key 不为 NONE 且 PressMode 不为 None |
| Mouse（按键） | Key 不为 NONE 且 PressMode 不为 None |
| Mouse（滚轮） | Key 不为 NONE 且倍率满足选项（`!= 0` 或 `> 0`） |
| Delay | 倍率 >= 15 |
| None | 永远不合法 |

---

## 共享控件一览

| 控件 | 用途 | 绑定什么 |
|------|------|---------|
| `KeyActionConfigV2Control` | 完整编辑器（含组合键按钮、动作列表） | `KeyActionConfigV2ViewModel` |
| `KeyActionConfigView` | 预设场景编辑器（Expander 样式） | `KeyActionConfigForPresetsViewModel` |
| `KeyActionsItemsControl` | 纯动作列表（无头部） | `IEnumerable<KeyActionWithCommandViewModel>` |
| `CombinationKeysConfigView` | 组合键输入框（修饰键复选框 + 主键输入） | `CombinationKeysConfigViewModel` |
| `CombinationKeyActionConfigControl` | 组合键配置控件 | `CombinationKeysWithCommandVM` |
| `RunningProgramSelector` | 运行程序选择弹窗 | `RunningProgramSelectorViewModel` |

---

## 常见问题

### Q: 编辑器显示出来了但 IsAvailable 始终为 false？

检查是否所有动作的 `ActionType`、`Key`、`PressMode` 都已填写。键盘和鼠标动作需要同时满足三者非空。

### Q: 用户切换 ActionType 后 PressMode 没有跟随模式变化？

你可能传入了固定的 PressMode 策略。如果项目的模式在运行时可以切换，参考教程 4 使用 `DelegatingPressModePolicy`。

### Q: 滚轮动作的负倍率被标记为不可用？

默认的 `KeyActionAvailabilityValidationOptions` 是 `RequirePositiveScrollMultiplier: false`（允许负值）。如果你继承了 `KeyActionConfigEditableBaseViewModel` 并覆盖了 `GetAvailabilityOptions()` 返回 `true`，就会要求倍率 > 0。检查你的覆盖。

### Q: 命令按钮（添加/删除/插入）点击没反应？

`KeyActionWithCommandViewModel` 的三个命令（`RemoveActionConfigCommand` 等）由父 VM 在工厂方法中绑定。如果你直接 `new KeyActionWithCommandViewModel(...)` 而不是通过 `KeyActionConfigEditableBaseViewModel.CreateManagedActionConfigVm()` 创建，这三个命令会是 `null`。

### Q: 如何在控件中隐藏 PressMode 选择器？

`KeyActionsItemsControl` 不支持 `IsSingleActionMode` 属性。如果你的项目不需要显示 PressMode，使用 `SingleActionKeyActionPressModePolicy` 并在 AXAML 中通过可见性绑定隐藏 PressMode 区域。
