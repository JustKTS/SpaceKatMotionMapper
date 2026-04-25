using System;
using System.Linq;
using Ursa.Controls;
var type = typeof(AutoCompleteBox);
Console.WriteLine(type.FullName);
Console.WriteLine("-- Events --");
foreach (var evt in type.GetEvents().OrderBy(e => e.Name))
    Console.WriteLine(evt.Name);
Console.WriteLine("-- Properties --");
foreach (var prop in type.GetProperties().Where(p => p.Name.Contains("Select", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Text", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Drop", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Item", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p.Name))
    Console.WriteLine($"{prop.Name} : {prop.PropertyType.FullName}");
Console.WriteLine("-- Fields --");
foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(f => f.Name.Contains("Select", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Text", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Drop", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Item", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Name))
    Console.WriteLine($"{field.Name} : {field.FieldType.FullName}");
