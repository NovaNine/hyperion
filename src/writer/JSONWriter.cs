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
  /// <summary>
  /// JSONWriter implements the IDataWriter interface for writing data to a JSON file.
  /// </summary>
  public class JSONWriter : IDataWriter
  {
    /// <summary>
    /// The file path where the JSON data will be saved.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// The root JSON object that will be written to the file.
    /// </summary>
    private JObject _rootObject;

    /// <summary>
    /// Flag indicating whether the JSON data has been modified since the last save.
    /// </summary>
    private bool _dirty;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    public JSONWriter(string filePath)
    {
      _filePath = filePath;
      _rootObject = new JObject();
      _dirty = false;
    }

    /// <summary>
    /// Inserts a record into a JSON data array at the specified path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="record"></param>
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

    /// <summary>
    /// Inserts a record into the "history" array in the JSON data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="record"></param>
    public void Insert<T>(T record)
    {
      this.Insert("history", record);
    }

    /// <summary>
    /// Sets a value at the specified path in the JSON data.
    /// If the path does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="value"></param>
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

    /// <summary>
    /// Splits a path string into an array of segments.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string[] SplitPath(string path)
    {
      return path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Ensures that a path exists in the JSON data.
    /// If the path does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private T EnsurePathExists<T>(string path) where T : JToken, new()
    {
      var segments = SplitPath(path);
      return EnsurePathExists<T>(segments);
    }

    /// <summary>
    /// Ensures that a path exists in the JSON data.
    /// If the path does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="segments"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Flushes the JSON data to the file.
    /// </summary>
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

    /// <summary>
    /// Disposes of the JSONWriter, flushing any unsaved data to the file.
    /// </summary>
    public void Dispose()
    {
      Flush();
    }
  }
}
