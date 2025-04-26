// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Hyperion.Writer
{
  public class JSONWriter : IDataWriter
  {
    private readonly string _filePath;
    private JObject _rootObject;
    private bool _dirty;

    public JSONWriter(string filePath)
    {
      _filePath = filePath;
      _rootObject = new JObject();
      _dirty = false;
    }

    public void Insert<T>(string path, T record)
    {
      if (record == null || string.IsNullOrWhiteSpace(path))
        return;
    }

    public void Insert<T>(T record)
    {
      if (record == null)
        return;

      try
      {
        // Serialize the single record
        string json = JsonConvert.SerializeObject(record, Formatting.None);

        // Append to file (plus newline to separate records)
        _streamWriter.WriteLine(json);
      }
      catch (Exception ex)
      {
        // Handle or log the error if needed
        Console.WriteLine($"[ERROR] Failed to write record: {ex.Message}");
      }
    }

    private string[] SplitPath(string path)
    {
      return path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private JArray GetOrCreateArrayAtPath(string path)
    {
      var segments = SplitPath(path);

      JToken current = _rootObject;

      for (int i = 0; i < segments.Length; i++)
      {
        string segment = segments[i];

        if (current.Type != JTokenType.Object)
        {
          throw new InvalidOperationException($"Path traversal error: '{string.Join(".", segments, 0, i)}' is not an object.");
        }

        var currentObject = (JObject)current;

        if (currentObject[segment] == null)
        {
          if (i == segments.Length - 1)
          {
            // Last segment — create array
            var newArray = new JArray();
            currentObject[segment] = newArray;
            return newArray;
          }
          else
          {
            // Intermediate segment — create object
            var newObject = new JObject();
            currentObject[segment] = newObject;
            current = newObject;
          }
        }
        else
        {
          current = currentObject[segment];

          if (i == segments.Length - 1)
          {
            if (current.Type == JTokenType.Array)
            {
              return (JArray)current;
            }
            else
            {
              throw new InvalidOperationException($"Path error: '{path}' exists but is not an array.");
            }
          }
        }
      }

      throw new InvalidOperationException($"Failed to navigate path '{path}'.");
    }

    public void Flush()
    {
      if (!_dirty)
        return;

      try
      {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        File.WriteAllText(_filePath, _rootObject.ToString(Formatting.Indented));
        _dirty = false;
      }
      catch (Exception ex)
      {
        Output.Error($"[Hyperion] Failed to save JSON data: {ex.Message}");
      }
    }

    public void Dispose()
    {
      Flush();
    }
  }
}
