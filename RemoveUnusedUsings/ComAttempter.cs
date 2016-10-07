using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RemoveUnusedUsings
{
    public static class ComAttempter
    {
        public static TOut Try<TOut>(Func<TOut> func, int times = 3, int delayMs = 1000)
        {
            for (var i = 0; i < times - 1; ++i)
            {
                try
                {
                    return func();
                }
                catch (COMException)
                {
                    Thread.Sleep(delayMs);
                }
            }

            return func();
        }

        public static void Try(Action action, int times = 3, int delayMs = 1000)
        {
            for (var i = 0; i < times - 1; ++i)
            {
                try
                {
                    action();
                    return;
                }
                catch (COMException)
                {
                    Thread.Sleep(delayMs);
                }
            }

            action();
        }
    }
}