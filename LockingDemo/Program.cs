/*  Show the impact of locking on performance when compared to running without locks.
 * Note that the performance impact of acquiring a lock is greatly overshadowed when
 * threads are blocked rather than the cost of act of locking
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var demo1 = new LockDemo();
            demo1.ShowBasicLocking();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            demo1.ShowInterlockVsLocking();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }

    class LockDemo
    {
        public void ShowBasicLocking()
        {
            var stopwatch = new Stopwatch();
            const int LoopCount = (int)(100 * 1e6);
            int counter = 0;

            for (int repetition = 0; repetition < 5; repetition++) {
                stopwatch.Reset();
                stopwatch.Start();
                // Acquire a Monitor 100 million times without contention
                for (int i = 0; i < LoopCount; i++)
                    lock (stopwatch)
                        counter = i;
                stopwatch.Stop();
                Console.WriteLine("With lock: {0}", stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
                stopwatch.Start();
                // Get base time for looping and assigning
                for (int i = 0; i < LoopCount; i++)
                    counter = i;
                stopwatch.Stop();
                Console.WriteLine("Without lock: {0}", stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Interlock uses operating system and CPU level mechanisms to guarantee atomic operations
        /// It is faster than locking
        /// </summary>
        public void ShowInterlockVsLocking()
        {
            var stopwatch = new Stopwatch();
            const int LoopCount = (int)(100 * 1e6);
            int counter = 0;

            for (int repetition = 0; repetition < 5; repetition++) {
                stopwatch.Reset();
                stopwatch.Start();
                // We repeatedly put i into the counter (and ignore the previous value)
                for (int i = 0; i < LoopCount; i++)
                    Interlocked.Exchange(ref counter, i);

                stopwatch.Stop();
                Console.WriteLine("Using Interlock: {0}", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
