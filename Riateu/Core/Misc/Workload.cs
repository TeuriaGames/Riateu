using System;
using System.Collections.Generic;
using System.Threading;

namespace Riateu;

public delegate bool WorkdloadHandler(int groupID, int threadID);
public class Workload(WorkdloadHandler worker, int chunks)
{
    private WorkdloadHandler worker = worker;
    private int chunks = chunks;

    public static bool IsSingleThreaded = false;
    public static int GetCompatibleThreadCount(int threadCount) 
    {
        if (IsSingleThreaded) 
        {
            return 1;
        }
        return threadCount;
    }

    public bool FinishSequential() 
    {
        for (int i = 0; i < chunks; i++) 
        {
            if (!worker(i, 0)) 
            {
                return false;
            }
        }

        return true;
    }

    public bool FinishParallel(int threadCount) 
    {
        bool result = true;
        int next = 0;
        Action threadWorker = () => {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            int i = next;
            while (result && i < chunks) 
            {
                if (!worker(i, threadID)) 
                {
                    result = false;
                }
                next = Interlocked.Increment(ref next);
                i = next;
            }
        };

        List<Thread> threads = new List<Thread>(threadCount);

        for (int i = 0; i < threadCount; i++) 
        {
            Thread thread = new Thread(() => threadWorker());
            threads.Add(thread);
            thread.Start();
        }
        for (int i = 0; i < threads.Count; i++) 
        {
            Thread thread = threads[i];
            thread.Join();
        }

        return result;
    }

    public bool Finish(int threadCount) 
    {
        if (Workload.IsSingleThreaded) 
        {
            threadCount = 1;
        }
        if (chunks == 0) 
        {
            return true;
        }
        if (threadCount == 1 || chunks == 1) 
        {
            return FinishSequential();
        }
        if (threadCount > 1) 
        {
            return FinishParallel(Math.Min(threadCount, chunks));
        }
        return false;
    }
}