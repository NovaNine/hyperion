// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Hyperion.Writer
{
  /// <summary>
  /// WriterManager is responsible for managing multiple data writers and sending the data to all writers.
  /// </summary>
  public class WriterManager
  {
    /// <summary>
    /// List of data writers to be used.
    /// </summary>
    private List<IDataWriter> writers = new List<IDataWriter>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileName"></param>
    public WriterManager(string fileName)
    {
      var _filePath = Path.Combine(
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx")), fileName
      );

      // Add a LiteDbWriter to the list of writers
      //writers.Add(new LiteDbWriter(fileName + ".db"));

      // Add a JSONWriter to the list of writers
      writers.Add(new JSONWriter(_filePath + ".json"));
    }

    /// <summary>
    /// Inserts a record into all writers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="category"></param>
    /// <param name="record"></param>
    public void Insert<T>(string category, T record)
    {
      foreach (var writer in writers)
      {
        writer.Insert(category, record);
      }
    }

    /// <summary>
    /// Sets a record at path in all writers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="record"></param>
    public void Set<T>(string path, T record)
    {
      foreach (var writer in writers)
      {
        writer.Set(path, record);
      }
    }

    /// <summary>
    /// Flushes all writers to ensure all data is written to disk.
    /// </summary>
    public void Flush()
    {
      foreach (var writer in writers)
      {
        writer.Flush();
      }
    }

    /// <summary>
    /// Disposes of all writers to release resources.
    /// </summary>
    public void Dispose()
    {
      foreach (var writer in writers)
      {
        writer.Dispose();
      }
    }
  }
}