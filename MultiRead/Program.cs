/*  A simple demonstration of the .NET 4.5 threading techniques
 *      Tasks using async/await keywords
 *      Task.WaitAny
 *      Parallel extension
 *      Threadpool
 */

using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace MultiRead
{
    class Program
    {
        static void Main(string[] args)
        {
            ParallelIOUsingTasks();
            ParallelInvoke();
            UsingAThreadpool();
        }

        private static void ParallelIOUsingTasks()
        {

            List<Task<string>> tasks = new List<Task<string>>();

            Console.WriteLine("Kicking Off Web Readers\n");
            tasks.Add(Program.ReadWebSiteAsync("http://www.google.com"));
            tasks.Add(Program.ReadWebSiteAsync("http://www.yahoo.com"));
            tasks.Add(Program.ReadWebSiteAsync("http://www.bing.com"));
            tasks.Add(Program.ReadWebSiteAsync("http://www.ask.com"));
            Console.WriteLine("All tasks started. Current Status of the tasks...");
            tasks.ForEach(t => { Console.WriteLine(t.Status.ToString()); });

            // Remove each task as it finishes
            // In a robust program one should consider timeouts, error handling, cancellation handling
            // try catch finally blocks, etc.
            Stopwatch sw = Stopwatch.StartNew();
            while (tasks.Count > 0) {
                var taskIndex = Task.WaitAny(tasks.ToArray());
                Task<string> t = tasks[taskIndex];
                tasks.RemoveAt(taskIndex);
                Console.WriteLine(sw.Elapsed.ToString() + ": Task #" + taskIndex + "( ID=" + t.Id + " ) " + t.Result);
            }
        }

        private static void ParallelInvoke()
        {
            // Run the same tasks using the Parallel Extension but only allow 2 threads to run at once
            var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
            Stopwatch sw = Stopwatch.StartNew();
            Parallel.Invoke(options,
                () => ReadWebSiteSync(sw, "https://www.google.com"),
                () => ReadWebSiteSync(sw, "http://www.yahoo.com"),
                () => ReadWebSiteSync(sw, "http://www.bing.com"),
                () => ReadWebSiteSync(sw, "http://www.ask.com")
            );

            Console.WriteLine(sw.Elapsed.ToString() + ": All Parallel Threads Finished");

            
        }

        private static void UsingAThreadpool()
        {
            // Run Compute Tasks using the ThreadPool mechanism
            int nTasks = 10;
            ManualResetEvent[] doneEvents = new ManualResetEvent[nTasks];
            ComputeTask[] computeTasks = new ComputeTask[nTasks];
            Random r = new Random();
            int min, minIgnored, max, maxIgnored;

            ThreadPool.GetMinThreads(out min, out minIgnored);
            ThreadPool.GetMaxThreads(out max, out maxIgnored);
            Console.WriteLine("Starting " + nTasks + " Computation Events (Min=" + min + ", max=" + max);
            for(int i = 0; i < nTasks; ++i) {
                doneEvents[i] = new ManualResetEvent(false);
                computeTasks[i] = new ComputeTask(r.Next(5000000,100000000), doneEvents[i]);
                ThreadPool.QueueUserWorkItem(computeTasks[i].RunTask, i);
            }
            // Wait for all the threads to signal that they are done
            WaitHandle.WaitAll(doneEvents);
            Console.WriteLine("All ComputeTasks have finished");
        }


        #region Support Routines

        static async Task<string> ReadWebSiteAsync(string url)
        {
            var client = new HttpClient();
            Task<string> urlTask = client.GetStringAsync(url);
            string urlContents = await urlTask;
            
            return "Thread #" + Thread.CurrentThread.ManagedThreadId + " read "
                + url + " and got " + urlContents.Length + " bytes";
        }


        static void ReadWebSiteSync(Stopwatch sw, string url)
        {
            var client = new HttpClient();
            Task<string> t = client.GetStringAsync(url);
            t.Wait();
            // Not sure we really need to lock the stopwatch because we are reading a single value
            var finishedTime = "";
            lock (sw) {
                finishedTime = sw.Elapsed.ToString();
            }
            Console.WriteLine(finishedTime +  ": Thread #" + Thread.CurrentThread.ManagedThreadId + " read "
                + url + " and got " + t.Result.Length + " bytes" );
        }

        #endregion
    }

    class ComputeTask
    {
        private int _maxCount;
        private ManualResetEvent _doneEvent;

        public ComputeTask(int count, ManualResetEvent doneEvent) {
            _maxCount = count;
            _doneEvent = doneEvent;
        }

        public void RunTask(Object context)
        {
            double sum = 0.0;
            for (int i = 1; i <= _maxCount; ++i) {
                sum += Math.Log(i) * Math.Log10(i);
            }
            _doneEvent.Set();
            Console.WriteLine("ComputeTask " + ((int)context).ToString() + ": Log Sum(" + _maxCount + "=" + sum.ToString("e15"));
        }
    }
}
