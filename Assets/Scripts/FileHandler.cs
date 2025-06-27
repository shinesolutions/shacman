using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public static class FileHandler
{
    public static void SaveToJSON<T>(List<T> toSave, string fileName)
    {
        Debug.Log(GetPath(filename))
        string content = JsonHelper.ToJson<T>(toSave.ToArray());
        WriteFile(GetPath(fileName), content);
    }

    public static List<T> ReadFromJSON(string filename)
    {
        string content = ReadFile(GetPath(filename));
        
        if (string.IsNullOrEmpty(content) || content = "{}") {
            return new List<T>();
        }

        List<T> response = JsonHelper.FromJson<T>(content).ToList;

        return response;
    }

    private static string GetPath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;

    }

    private static void WriteFile(string path, string content)
    {
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(content);
        }
    }

    private static string ReadFile(string path)
    {
        if(File.Exists(path)) {
            using(StreamReader reader = new StreamReader(path)) {
                string content = reader.ReadToEnd();
                return content;
            }
        }
        return "";
    }
}