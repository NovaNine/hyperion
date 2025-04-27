// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Profiling;

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

      try
      {
        var array = EnsurePathExists<JArray>(path);
        array.Add(JToken.FromObject(record));
        _dirty = true;
      }
      catch (Exception ex)
      {
        Output.Error($"Failed to insert into JSON at path '{path}': {ex.Message}");
      }

      Output.Trace($"JSON Insert at {path}: {record}");
    }

    public void Insert<T>(T record)
    {
      this.Insert("history", record);
    }

    public void Set<T>(string path, T value)
    {
      if (string.IsNullOrWhiteSpace(path))
        return;

      var segments = SplitPath(path);

      try
      {
        var parentPath = segments.Take(segments.Length - 1).ToArray();
        var parent = EnsurePathExists<JObject>(parentPath);
        parent[segments.Last()] = JToken.FromObject(value);
        _dirty = true;
      }
      catch (Exception ex)
      {
        Output.Error($"Failed to set JSON at path '{path}': {ex.Message}");
      }

      Output.Trace($"JSON Set: {path} = {value}");
    }

    private string[] SplitPath(string path)
    {
      return path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private T EnsurePathExists<T>(string path) where T : JToken, new()
    {
      var segments = SplitPath(path);
      return EnsurePathExists<T>(segments);
    }

    private T EnsurePathExists<T>(string[] segments) where T : JToken, new()
    {
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
            var newNode = new T();
            currentObject[segment] = newNode;
            return newNode;
          }
          else
          {
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
            if (!(current is T))
              throw new InvalidOperationException($"Path error: '{string.Join(".", segments)}' exists but is not a {typeof(T).Name}.");
          }
        }
      }

      return (T)current;
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
        Output.Trace($"JSON Flush");
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
