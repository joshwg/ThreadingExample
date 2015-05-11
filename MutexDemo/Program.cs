using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace MutexDemo
{
    // Run at least 2 of these
    class MutexExample
    {

        const int THREAD_COUNT = 3;
        static void Main(string[] args)
        {
            var theMutex = new Mutex();

            ManualResetEvent[] waitEvents = new ManualResetEvent[THREAD_COUNT];

            // Demonstrate using Thread and ThreadStart
            // We create each thread; name it and then start
            for (int i = 0; i < THREAD_COUNT; ++i) {
                waitEvents[i] = new ManualResetEvent(false);
                Runner r = new Runner("Thread" + (i+1), waitEvents[i], theMutex);
                Task t = new Task(() => r.ThreadProcess());
                t.Start();
            }
            // Wait for all the other threads before exiting the main thread.
            // Could wait for the tasks directly via Task.WaitAll(tasklist)
            WaitHandle.WaitAll(waitEvents);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }

    class Runner
    {
        const int ITERATIONS = 10;

        private String _name;
        private ManualResetEvent _doneEvent;
        private Mutex _mutex; // shared reference to the mutex

        public Runner(String name, ManualResetEvent doneEvent, Mutex theMutex)
        {
            _name = name;
            _doneEvent = doneEvent;
            _mutex = theMutex;
        }

        public void ThreadProcess()
        {
            for (int iteration = 1; iteration <= ITERATIONS; ++iteration) {
                Console.WriteLine(String.Format("{0} iteration {1}", _name, iteration));
                UseResource();
            }
            Console.WriteLine(String.Format("{0} is exiting", _name));
            _doneEvent.Set();
        }

        // This method represents a resource that must be synchronized 
        // so that only one thread at a time can enter. 
        private void UseResource()
        {
            // Wait until it is safe to enter, and do not enter if the request times out.
            Console.WriteLine("{0} is requesting the mutex", _name);
            if (_mutex.WaitOne(1000)) {
                Console.WriteLine(">>>{0} has entered the protected area", _name);

                // Place code to access non-reentrant resources here. 

                // Simulate some work.
                Thread.Sleep(3000);

                Console.WriteLine("<<< {0} is leaving the protected area", _name);

                // Release the Mutex.
                _mutex.ReleaseMutex();
                Console.WriteLine("{0} has released the mutex", _name);
            }
            else {
                Console.WriteLine("{0} will not acquire the mutex", _name);
            }
        }
    }
}
