﻿using System;
using System.IO;
using System.Text;
using System.Text.Json;
using SpaceKatMotionMapper.Helpers;
using SpaceKatMotionMapper.Services.Contract;

namespace SpaceKatMotionMapper.Services;

public class FileService : IFileService
{
    public T? Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (!File.Exists(path)) return default;
        var json = File.ReadAllText(path);
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonSgOption.Default);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return default;
        }
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonSerializer.Serialize(content, JsonSgOption.Default);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }

    public void Delete(string folderPath, string? fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}