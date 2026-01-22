using System;
using System.Collections.Generic;

public static class GlobalDelegate
{
    private static readonly Dictionary<Type, Delegate> eventTable = new();

    public static void Subscribe<T>(Action<T> listener)
    {
        var type = typeof(T);
        if (eventTable.TryGetValue(type, out var del))
            eventTable[type] = Delegate.Combine(del, listener);
        else
            eventTable[type] = listener;
    }

    public static void Unsubscribe<T>(Action<T> listener)
    {
        var type = typeof(T);
        if (eventTable.TryGetValue(type, out var del))
        {
            var result = Delegate.Remove(del, listener);
            if (result == null)
                eventTable.Remove(type); // 더 이상 리스너가 없으면 키 삭제 (메모리 관리)
            else
                eventTable[type] = result;
        }
    }

    public static void Raise<T>(T args)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            //안전한 캐스팅 추가
            (del as Action<T>)?.Invoke(args);
        }
    }
}