// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9
using System;

namespace Hyperion.Writer
{
  /// <summary>
  /// Interface for data writers.
  /// </summary>
  /// <remarks>
  /// Implementations must provide a constructor that accepts a string path.
  /// </remarks>
  public interface IDataWriter : IDisposable
  {
    void Insert<T>(string path, T record);
    void Set<T>(string path, T record);
    void Flush();
  }
}
