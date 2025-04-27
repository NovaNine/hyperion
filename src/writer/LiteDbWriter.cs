// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class LiteDbWriter
{
  private LiteDatabase db;

  public LiteDbWriter(string filePath)
  {
    db = new LiteDatabase(filePath);
  }

  public void Insert<T>(T record)
  {
    this.Insert("history", record);
  }

  public void Insert<T>(string collectionName, T record)
  {
    var collection = db.GetCollection<T>(collectionName);
    collection.Insert(record);
  }

  public void Flush()
  {
    db.Checkpoint();
  }

  public void Dispose()
  {
    db?.Dispose();
  }
}
