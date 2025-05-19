using System;
using System.Collections;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM.Physics;
using UnityEngine;

namespace ScarletRCON;

public static class CoroutineHandler {
  private static readonly IgnorePhysicsDebugSystem coroutineManager = new GameObject("ScarletModsCoroutine").AddComponent<IgnorePhysicsDebugSystem>();

  public static T ExecuteAsync<T>(Func<T> func) {
    if (coroutineManager == null) {
      Core.Log.LogError("CoroutineManager not initialized.");
      return default;
    }
    var tcs = new TaskCompletionSource<T>();
    coroutineManager.StartCoroutine(ExecuteWithResult(func, tcs).WrapToIl2Cpp());
    return tcs.Task.GetAwaiter().GetResult();
  }

  private static IEnumerator ExecuteWithResult<T>(Func<T> func, TaskCompletionSource<T> tcs) {
    yield return null;
    try {
      var result = func();
      tcs.SetResult(result);
    } catch (Exception ex) {
      tcs.SetException(ex);
    }
  }
}
