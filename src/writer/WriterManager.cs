// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using PavonisInteractive.TerraInvicta.Actions;

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
    /// The file name for the data writers.
    /// </summary>
    private string _fileName = null;

    /// <summary>
    /// The full file path for the data writers.
    /// </summary>
    private string _filePath = null;

    private Queue<PendingOperation> pendingOperations = new Queue<PendingOperation>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public WriterManager()
    {

    }

    /// <summary>
    /// Convenience constructor that also takes a file name.
    /// </summary>
    /// <param name="fileName"></param>
    public WriterManager(string fileName)
    {
      SetFilename(fileName);
    }

    /// <summary>
    /// Sets the filename (and file path) if they have not already been set and creates the writers.
    /// </summary>
    /// <param name="fileName"></param>
    public void SetFilename(string fileName)
    {
      if (writers.Count > 0)
      {
        if (fileName != _fileName)
        {
          Output.Error("Writers have already been created. Cannot change filename.");
        }
        else
        {
          Output.Debug($"Writers have already been created with same filename ({_fileName})");
        }
        return;
      }

      _fileName = fileName;

      _filePath = Path.Combine(
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx")), _fileName
      );

      CreateWriters();
    }

    /// <summary>
    /// Creates the writers based on the file path set.
    /// </summary>
    private void CreateWriters()
    {
      if (string.IsNullOrEmpty(_filePath))
      {
        Output.Debug("Filename is not set. Cannot create writers.");
        return;
      }

      // Add a LiteDbWriter to the list of writers
      //writers.Add(new LiteDbWriter(_filePath + ".db"));

      // Add a JSONWriter to the list of writers
      writers.Add(new JSONWriter(_filePath + ".json"));

      // Empty the operationsn buffer
      SendPendingOperationsToWriters();
    }

    /// <summary>
    /// Inserts a record into all writers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="category"></param>
    /// <param name="record"></param>
    public void Insert<T>(string category, T record)
    {
      // Bufferingn if no writers are available
      if (writers.Count == 0)
      {
        pendingOperations.Enqueue(new PendingOperation
        {
          OperationType = PendingOperationType.Insert,
          PathOrCategory = category,
          Record = record
        });
        return;
      }

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
      // Buffering if no writers are available
      if (writers.Count == 0)
      {
        pendingOperations.Enqueue(new PendingOperation
        {
          OperationType = PendingOperationType.Set,
          PathOrCategory = path,
          Record = record
        });
        return;
      }

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
    /// Sends any pending operations to the writers.
    /// </summary>
    public void SendPendingOperationsToWriters()
    {
      if (pendingOperations == null || pendingOperations.Count == 0)
        return;

      while (pendingOperations.Count > 0)
      {
        var op = pendingOperations.Peek();

        switch (op.OperationType)
        {
          case PendingOperationType.Insert:
            {
              Insert(op.PathOrCategory, op.Record);
              break;
            }
          case PendingOperationType.Set:
            {
              Set(op.PathOrCategory, op.Record);
              break;
            }
          default:
            {
              Output.Error($"WriterManager trying to replay unknown operation type {op.OperationType}");
              break;
            }
        }

        pendingOperations.Dequeue();
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

    /// <summary>
    /// Enumeration for pending operation types.
    /// </summary>
    private enum PendingOperationType
    {
      Insert,
      Set
    }

    /// <summary>
    /// Class representing a pending operation.
    /// </summary>
    private class PendingOperation
    {
      public PendingOperationType OperationType;
      public string PathOrCategory;
      public object Record;
    }
  }
}