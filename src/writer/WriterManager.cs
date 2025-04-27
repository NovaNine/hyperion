// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Hyperion.Writer
{
  public class WriterManager
  {
    private List<IDataWriter> writers = new List<IDataWriter>();
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

    public void Insert<T>(string category, T record)
    {
      foreach (var writer in writers)
      {
        writer.Insert(category, record);
      }
    }

    public void Set<T>(string path, T record)
    {
      foreach (var writer in writers)
      {
        writer.Set(path, record);
      }
    }

    public void Flush()
    {
      foreach (var writer in writers)
      {
        writer.Flush();
      }
    }

    public void Dispose()
    {
      foreach (var writer in writers)
      {
        writer.Dispose();
      }
    }
  }
}