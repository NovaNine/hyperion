using System;
using System.Reflection;

namespace Hyperion {
  public static class Util{
    public static object GetPrivateField(object obj, string fieldName) {
      return obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
    }

    public static T GetPrivateField<T>(object obj, string fieldName) {
      return (T)GetPrivateField(obj, fieldName);
    }
  }
}