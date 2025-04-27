// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hyperion.Writer
{
  /// <summary>
  /// LiteDbWriter implements IDataWriter to write data to a LiteDB database.
  /// </summary>
  public class LiteDbWriter
  {
    /// <summary>
    /// The LiteDB database instance.
    /// </summary>
    private LiteDatabase db;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    public LiteDbWriter(string filePath)
    {
      db = new LiteDatabase(filePath);
    }

    /// <summary>
    /// Inserts a record into the "history" collection in the db.
    /// If the collection does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="record"></param>
    public void Insert<T>(T record)
    {
      this.Insert("history", record);
    }

    /// <summary>
    /// Inserts a record into the specified collection in the db.
    /// If the collection does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collectionName"></param>
    /// <param name="record"></param>
    public void Insert<T>(string collectionName, T record)
    {
      var collection = db.GetCollection<T>(collectionName);
      collection.Insert(record);
    }

    /// <summary>
    /// Flushes the database to ensure all changes are written to disk.
    /// </summary>
    public void Flush()
    {
      db.Checkpoint();
    }

    /// <summary>
    /// Disposes of the LiteDB database instance.
    /// </summary>
    public void Dispose()
    {
      Flush();
      db?.Dispose();
    }
  }
}
