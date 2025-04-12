using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using LanguageExt.Common;
using Serilog;
using SpaceKat.Shared.Helpers;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Models;
using WindowsInput;

namespace SpaceKatMotionMapper.Functions;

public static class OfficialWareConfigFunctions
{
    private static readonly string ConfigDirPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        // ReSharper disable once StringLiteralTypo
        "3Dconnexion", "3DxWare", "Cfg");

    private static readonly string ConfigFilePath = Path.Combine(ConfigDirPath, "Global.xml");

    private const string AxisBankXml = """
                                       <AxisBank>
                                           <Name>Default</Name>
                                           <ID>Default</ID>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_Rx</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_Rx</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_Ry</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_Ry</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_Rz</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_Rz</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_X</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_X</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_Y</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_Y</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                           <Axis>
                                               <Enabled>false</Enabled>
                                               <Input>
                                                   <ActionID>HIDMultiAxis_Z</ActionID>
                                                   <Min>-512</Min>
                                                   <Max>511</Max>
                                               </Input>
                                               <Output>
                                                   <ActionID>HIDMultiAxis_Z</ActionID>
                                                   <Reversed>false</Reversed>
                                               </Output>
                                           </Axis>
                                       </AxisBank>
                                       """;

    private const string DevicesXml = """
                                      <Devices>
                                          <Device>
                                              <ID>ID_Standard_3D_Mouse</ID>
                                              <Name>Standard 3D Mouse</Name>
                                              <Settings>
                                                  <OnScreenDisplay>
                                                      <Enabled>true</Enabled>
                                                      <AlwaysOn>true</AlwaysOn>
                                                  </OnScreenDisplay>
                                              </Settings>
                                          </Device>
                                      </Devices>
                                      """;

    private static XmlDocument LoadDocument()
    {
        var doc = new XmlDocument();
        doc.Load(ConfigFilePath);
        return doc;
    }
    
    public static async Task<Result<bool>> OpenOfficialMapper()
    {
        var doc = LoadDocument();

        var node1 = doc.SelectSingleNode("Global");
        if (node1 == null) return new Result<bool>(new Exception("配置文件解析失败，请检查配置文件是否有被不正确的修改！"));
        var node2 = node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.SelectSingleNode("AxisBank");
        if (node2 == null) return true;
        node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.RemoveChild(node2);
        try
        {
            doc.Save(ConfigFilePath);
        }
        catch (Exception e)
        {
            await Task.Delay(500); // 猜测是官方驱动会自动加载文件，导致文件被占用无法写入，延迟半秒后再操作。
            doc.Save(ConfigFilePath);
        }
        return true;
    }

    public static async Task<Result<bool>> UnbindHotKeyToKatButton()
    {
        var doc = LoadDocument();
        var node1 = doc.SelectSingleNode("Global");
        if (node1 == null) return new Result<bool>(new Exception("配置文件解析失败，请检查配置文件是否有被不正确的修改！"));
        var node2 = node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.SelectSingleNode("ButtonBank");
        if (node2 != null)
        {
            node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.RemoveChild(node2);
        }

        var node3 = node1.SelectSingleNode("MacroTable");
        if (node3 != null)
        {
            node1.RemoveChild(node3);
        }

        try
        {
            doc.Save(ConfigFilePath);
        }
        catch (Exception e)
        {
            await Task.Delay(500); // 猜测是官方驱动会自动加载文件，导致文件被占用无法写入，延迟半秒后再操作。
            doc.Save(ConfigFilePath);
        }
        return true;
    }

    public static async Task<Result<bool>> BindHotKeyToKatButton(
        KatButtonEnum button, bool useCtrl, bool useAlt, bool useShift, VirtualKeyCode hotKey)
    {
        var doc = LoadDocument();
        var node1 = doc.SelectSingleNode("Global");
        if (node1 == null) return new Result<bool>(new Exception("配置文件解析失败，请检查配置文件是否有被不正确的修改！"));
        var node2 = node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.SelectSingleNode("ButtonBank");
        if (node2 != null)
        {
            node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.RemoveChild(node2);
        }

        var node3 = node1.SelectSingleNode("MacroTable");
        if (node3 != null)
        {
            node1.RemoveChild(node3);
        }


        var buttonDoc = CreateButtonXml(button);
        var macroDoc = CreateHotKeyMacroXml(useCtrl, useAlt, useShift, hotKey);

        var buttonNode = doc.ImportNode(buttonDoc.SelectSingleNode("ButtonBank")!, true);
        node1.SelectSingleNode("Devices")?.SelectSingleNode("Device")?.AppendChild(buttonNode);
        var marcoNode = doc.ImportNode(macroDoc.SelectSingleNode("MacroTable")!, true);
        node1.AppendChild(marcoNode);

        while (FileOccupiedChecker.IsOccupied(ConfigFilePath))
        {
            await Task.Delay(200);
        }

        try
        {
            doc.Save(ConfigFilePath);
        }
        catch (Exception e)
        {
            await Task.Delay(500); // 猜测是官方驱动会自动加载文件，导致文件被占用无法写入，延迟半秒后再操作。
            doc.Save(ConfigFilePath);
        }
       
        return true;
    }

    public static async Task<Result<bool>> CloseOfficialMapper()
    {
        var doc = LoadDocument();
        var node1 = doc.SelectSingleNode("Global");
        if (node1 == null) return new Result<bool>(new Exception("配置文件解析失败，请检查配置文件是否有被不正确的修改！"));
        var node2 = node1.SelectSingleNode("Devices");
        if (node2 == null)
        {
            var docDevices = new XmlDocument();
            docDevices.LoadXml(DevicesXml);
            var importNode1 = doc.ImportNode(docDevices.SelectSingleNode("Devices")!, true);
            node1.AppendChild(importNode1);
        }

        node2 = node1.SelectSingleNode("Devices");
        var node3 = node2!.SelectSingleNode("Device");
        var docAxis = new XmlDocument();
        docAxis.LoadXml(AxisBankXml);
        var importNode2 = doc.ImportNode(docAxis.SelectSingleNode("AxisBank")!, true);
        node3!.AppendChild(importNode2);
        try
        {
            doc.Save(ConfigFilePath);
        }
        catch (Exception e)
        {
            await Task.Delay(500); // 猜测是官方驱动会自动加载文件，导致文件被占用无法写入，延迟半秒后再操作。
            doc.Save(ConfigFilePath);
        }
        return true;
    }

    public static async Task<Result<bool>> CleanAllChange()
    {
        var doc = LoadDocument();
        var node1 = doc.SelectSingleNode("Global");
        if (node1 == null) return new Result<bool>(new Exception("配置文件解析失败，请检查配置文件是否有被不正确的修改！"));
        var node2 = node1.SelectSingleNode("Devices");
        if (node2 != null)
        {
            node1.RemoveChild(node2);
        }

        var node3 = node1.SelectSingleNode("MacroTable");
        if (node3 != null)
        {
            node1.RemoveChild(node3);
        }

        try
        {
            doc.Save(ConfigFilePath);
        }
        catch (Exception e)
        {
            await Task.Delay(500); // 猜测是官方驱动会自动加载文件，导致文件被占用无法写入，延迟半秒后再操作。
            doc.Save(ConfigFilePath);
        }
        return true;
    }

    # region 绑定热键到SpaceKat按钮

    private const string ButtonBank =
        """
            <ButtonBank Default="true">
                <Name>STR_DEFAULT_BUTTONBANK</Name>
                <ID>Default</ID>
            </ButtonBank>
        """;

    private const string XmlHotKeyId = "MapperHotKey";

    private static XmlDocument CreateButtonXml(KatButtonEnum katButton)
    {
        var d1 = new XmlDocument();
        d1.LoadXml(ButtonBank);
        var buttonXml = d1.CreateElement("Button");
        var inputXml = d1.CreateElement("Input");
        var outputXml = d1.CreateElement("Output");
        var inputActionIdXml = d1.CreateElement("ActionID");
        inputActionIdXml.InnerText = katButton switch
        {
            KatButtonEnum.Left => "HIDButton_1",
            KatButtonEnum.Right => "HIDButton_2",
            KatButtonEnum.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(katButton), katButton, null)
        };
        var outputActionIdXml = d1.CreateElement("ActionID");
        outputActionIdXml.InnerText = XmlHotKeyId;
        var node1 = d1.SelectSingleNode("ButtonBank");
        outputXml.AppendChild(outputActionIdXml);
        inputXml.AppendChild(inputActionIdXml);
        buttonXml.AppendChild(inputXml);
        buttonXml.AppendChild(outputXml);
        node1!.AppendChild(buttonXml);
        return d1;
    }

    private static XmlDocument CreateHotKeyMacroXml(bool useCtrl, bool useAlt, bool useShift, VirtualKeyCode hotKey)
    {
        List<string> modifierKeyStrList = [];
        if (useCtrl)
        {
            modifierKeyStrList.Add("Control");
        }

        if (useAlt)
        {
            modifierKeyStrList.Add("Alt");
        }

        if (useShift)
        {
            modifierKeyStrList.Add("Shift");
        }

        XmlDocument d1 = new();
        var eMacroTable = d1.CreateElement("MacroTable");
        var eMacroEntry = d1.CreateElement("MacroEntry");
        var eId = d1.CreateElement("ID");
        eId.InnerText = XmlHotKeyId;
        var eKeyStroke = d1.CreateElement("KeyStroke");
        var eModifiers = d1.CreateElement("Modifiers");
        foreach (var modifierKey in modifierKeyStrList)
        {
            var eModifier = d1.CreateElement("Modifier");
            eModifier.InnerText = modifierKey;
            eModifiers.AppendChild(eModifier);
        }

        var eKey = d1.CreateElement("Key");
        eKey.InnerText = ((int)SpaceMouseXmlKeyEnumExtensions.Parse(
            hotKey.GetWrappedName(), true, true)).ToString("X");

        eMacroTable.AppendChild(eMacroEntry);
        eMacroEntry.AppendChild(eId);
        eMacroEntry.AppendChild(eKeyStroke);
        eKeyStroke.AppendChild(eModifiers);
        eKeyStroke.AppendChild(eKey);
        d1.AppendChild(eMacroTable);
        return d1;
    }

    #endregion
}