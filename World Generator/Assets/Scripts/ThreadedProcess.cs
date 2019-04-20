using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ThreadedProcess
{
    bool m_isDone = false;
    object m_Handle = new object();
    Thread m_thread = null;

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_isDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_isDone = value;
            }
        }
    }
    public virtual void Start()
    {
        Run();
        //m_thread = new Thread(Run);
        //m_thread.Start();
    }

    public virtual void Abort()
    {
        m_thread.Abort();
    }

    public virtual void ThreadFunction()
    {

    }

    public virtual void OnFinished()
    {

    }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    void Run()
    {
        ThreadFunction();
        IsDone = true;
    }
}
