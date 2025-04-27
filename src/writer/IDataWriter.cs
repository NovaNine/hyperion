// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9
using System;

public interface IDataWriter : IDisposable
{
  /// <summary>
  /// Implementations must provide a constructor: (string path)
  /// </summary>
  void Insert<T>(string path, T record);
  void Set<T>(string path, T record);
  void Flush();
}
