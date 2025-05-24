using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScarletRCON;

public static class CommandExecutor {
  public static int Count {
    get {
      lock (_queue) return _queue.Count;
    }
  }
  private static readonly Queue<(Func<object> func, TaskCompletionSource<object> tcs)> _queue = new();

  public static string Enqueue(Func<object> func) {
    var tcs = new TaskCompletionSource<object>();
    lock (_queue) _queue.Enqueue((func, tcs));
    return tcs.Task.GetAwaiter().GetResult()?.ToString();
  }

  public static void ProcessQueue() {
    while (true) {
      (Func<object> func, TaskCompletionSource<object> tcs) task;
      lock (_queue) {
        if (_queue.Count == 0) break;
        task = _queue.Dequeue();
      }

      try {
        var result = task.func.Invoke();
        task.tcs.SetResult(result);
      } catch (Exception ex) {
        task.tcs.SetException(ex);
      }
    }
  }
}
