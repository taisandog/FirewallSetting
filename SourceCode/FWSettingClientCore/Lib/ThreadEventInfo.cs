using Buffalo.ArgCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SettingLib
{
    /// <summary>
    /// 线程信息
    /// </summary>
    public class ThreadEventInfo
    {
        private Thread _thd;
        /// <summary>
        /// 线程
        /// </summary>
        public Thread CurrentThread
        {
            get
            {
                return _thd;
            }
        }
        private AutoResetEvent _handle;
        /// <summary>
        /// 锁的阻塞
        /// </summary>
        public AutoResetEvent Handle
        {
            get
            {
                return _handle;
            }

        }
        private bool _running = false;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool Running
        {
            get
            {
                return _running;
            }
            set 
            {
                _running = value;
            }
        }
        /// <summary>
        /// 押注线程信息
        /// </summary>
        /// <param name="thd"></param>
        /// <param name="arg"></param>
        public ThreadEventInfo(Thread thd)
        {
            _thd = thd;
            
            _handle = new AutoResetEvent(true);
            _handle.Reset();
        }


    
        /// <summary>
        /// 清空信息
        /// </summary>
        public void Clear()
        {
            _thd = null;
            if (_handle != null)
            {
                _handle.Close();
                _handle.Dispose();
            }
            _handle = null;
        }
        public void UnLock()
        {
            if (_handle != null)
            {
                _handle.Set();
            }
        }
        public bool Wait(int millisecondsTimeout)
        {
            if (_handle != null)
            {
                return _handle.WaitOne(millisecondsTimeout);
            }
            return true;
        }
    }
}
