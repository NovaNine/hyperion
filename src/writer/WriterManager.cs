// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using System;
using System.Collections.Generic;

namespace Hyperion.Writer
{
  public class WriterManager
  {
    private List<IDataWriter> writers = new List<IDataWriter>();
    public WriterManager(string filename) {
      // Add a LiteDbWriter to the list of writers
      writers.Add(new LiteDbWriter(filename));
      // Add a JSONWriter to the list of writers
      writers.Add(new JSONWriter(filename));
    }

    public void Insert<T>(T record)
    {
      foreach (var writer in writers)
      {
        writer.Insert(record);
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