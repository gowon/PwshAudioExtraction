namespace PwshAudioExtraction.Abstractions
{
    using System;
    using System.Collections.Concurrent;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;

    // https://github.com/ttrider/PowerShellAsync
    /// <summary>
    ///     Base class for async-enabled cmdlets
    /// </summary>
    public abstract class AsyncCmdlet : PSCmdlet
    {
        protected AsyncCmdlet(int boundedCapacity = 50)
        {
            BoundedCapacity = Math.Max(1, boundedCapacity);
        }

        protected int BoundedCapacity { get; set; }

        private class AsyncCmdletSynchronizationContext : SynchronizationContext, IDisposable
        {
            private static AsyncCmdletSynchronizationContext _currentAsyncCmdletContext;
            private BlockingCollection<MarshalItem> _workItems;

            private AsyncCmdletSynchronizationContext(int boundedCapacity)
            {
                _workItems = new BlockingCollection<MarshalItem>(boundedCapacity);
            }

            public void Dispose()
            {
                if (_workItems != null)
                {
                    _workItems.Dispose();
                    _workItems = null;
                }
            }

            public static void Async(Func<Task> handler, int boundedCapacity)
            {
                var previousContext = Current;

                try
                {
                    using (var synchronizationContext = new AsyncCmdletSynchronizationContext(boundedCapacity))
                    {
                        SetSynchronizationContext(synchronizationContext);
                        _currentAsyncCmdletContext = synchronizationContext;

                        var task = handler();
                        if (task == null)
                        {
                            return;
                        }

                        // ReSharper disable once AccessToDisposedClosure
                        var waitable = task.ContinueWith(t => synchronizationContext.Complete(), TaskScheduler.Default);

                        synchronizationContext.ProcessQueue();

                        waitable.GetAwaiter().GetResult();
                    }
                }
                finally
                {
                    SetSynchronizationContext(previousContext);
                    _currentAsyncCmdletContext = previousContext as AsyncCmdletSynchronizationContext;
                }
            }

            internal static void PostItem(MarshalItem item)
            {
                _currentAsyncCmdletContext.Post(item);
            }

            private void EnsureNotDisposed()
            {
                if (_workItems == null)
                {
                    throw new ObjectDisposedException(nameof(AsyncCmdletSynchronizationContext));
                }
            }

            private void Complete()
            {
                EnsureNotDisposed();

                _workItems.CompleteAdding();
            }

            private void ProcessQueue()
            {
                MarshalItem workItem;
                while (_workItems.TryTake(out workItem, Timeout.Infinite))
                {
                    workItem.Invoke();
                }
            }

            public override void Post(SendOrPostCallback callback, object state)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException(nameof(callback));
                }

                Post(new MarshalItemAction<object>(s => callback(s), state));
            }

            private void Post(MarshalItem item)
            {
                EnsureNotDisposed();

                _workItems.Add(item);
            }
        }

        #region sealed overrides

        protected sealed override void BeginProcessing()
        {
            AsyncCmdletSynchronizationContext.Async(BeginProcessingAsync, BoundedCapacity);
        }

        protected sealed override void ProcessRecord()
        {
            AsyncCmdletSynchronizationContext.Async(ProcessRecordAsync, BoundedCapacity);
        }

        protected sealed override void EndProcessing()
        {
            AsyncCmdletSynchronizationContext.Async(EndProcessingAsync, BoundedCapacity);
        }

        protected sealed override void StopProcessing()
        {
            AsyncCmdletSynchronizationContext.Async(StopProcessingAsync, BoundedCapacity);
        }

        #endregion sealed overrides

        #region intercepted methods

        public new void WriteDebug(string text)
        {
            AsyncCmdletSynchronizationContext.PostItem(new MarshalItemAction<string>(base.WriteDebug, text));
        }

        public new void WriteError(ErrorRecord errorRecord)
        {
            AsyncCmdletSynchronizationContext.PostItem(
                new MarshalItemAction<ErrorRecord>(base.WriteError, errorRecord));
        }

        public new void WriteObject(object sendToPipeline)
        {
            AsyncCmdletSynchronizationContext.PostItem(new MarshalItemAction<object>(base.WriteObject, sendToPipeline));
        }

        public new void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            AsyncCmdletSynchronizationContext.PostItem(
                new MarshalItemAction<object, bool>(base.WriteObject, sendToPipeline, enumerateCollection));
        }

        public new void WriteProgress(ProgressRecord progressRecord)
        {
            AsyncCmdletSynchronizationContext.PostItem(
                new MarshalItemAction<ProgressRecord>(base.WriteProgress, progressRecord));
        }

        public new void WriteVerbose(string text)
        {
            AsyncCmdletSynchronizationContext.PostItem(new MarshalItemAction<string>(base.WriteVerbose, text));
        }

        public new void WriteWarning(string text)
        {
            AsyncCmdletSynchronizationContext.PostItem(new MarshalItemAction<string>(base.WriteWarning, text));
        }

        public new void WriteCommandDetail(string text)
        {
            AsyncCmdletSynchronizationContext.PostItem(new MarshalItemAction<string>(base.WriteCommandDetail, text));
        }

        public new bool ShouldProcess(string target)
        {
            var workItem = new MarshalItemFunc<string, bool>(base.ShouldProcess, target);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult();
        }

        public new bool ShouldProcess(string target, string action)
        {
            var workItem = new MarshalItemFunc<string, string, bool>(base.ShouldProcess, target, action);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult();
        }

        public new bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            var workItem = new MarshalItemFunc<string, string, string, bool>(base.ShouldProcess, verboseDescription,
                verboseWarning, caption);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult();
        }

        public new bool ShouldProcess(string verboseDescription, string verboseWarning, string caption,
            out ShouldProcessReason shouldProcessReason)
        {
            var workItem = new MarshalItemFuncOut<string, string, string, bool, ShouldProcessReason>(
                base.ShouldProcess, verboseDescription, verboseWarning, caption);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult(out shouldProcessReason);
        }

        public new bool ShouldContinue(string query, string caption)
        {
            var workItem = new MarshalItemFunc<string, string, bool>(base.ShouldContinue, query, caption);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult();
        }

        public new bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            var workItem = new MarshalItemFuncRef<string, string, bool, bool, bool>(base.ShouldContinue, query, caption,
                yesToAll, noToAll);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult(ref yesToAll, ref noToAll);
        }

        public new bool TransactionAvailable()
        {
            var workItem = new MarshalItemFunc<bool>(base.TransactionAvailable);
            AsyncCmdletSynchronizationContext.PostItem(workItem);
            return workItem.WaitForResult();
        }

        public new void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            AsyncCmdletSynchronizationContext.PostItem(
                new MarshalItemAction<ErrorRecord>(base.ThrowTerminatingError, errorRecord));
        }

        #endregion

        #region async processing methods

        protected virtual Task BeginProcessingAsync()
        {
            return Task.FromResult(0);
        }


        protected virtual Task EndProcessingAsync()
        {
            return Task.FromResult(0);
        }


        protected virtual Task ProcessRecordAsync()
        {
            return Task.FromResult(0);
        }


        protected virtual Task StopProcessingAsync()
        {
            return Task.FromResult(0);
        }

        #endregion async processing methods

        #region items

        internal abstract class MarshalItem
        {
            internal abstract void Invoke();
        }

        abstract class MarshalItemFuncBase<TRet> : MarshalItem
        {
            private readonly Task<TRet> _retValTask;
            private TRet _retVal;

            protected MarshalItemFuncBase()
            {
                _retValTask = new Task<TRet>(() => _retVal);
            }

            internal sealed override void Invoke()
            {
                _retVal = InvokeFunc();
                _retValTask.Start();
            }

            internal TRet WaitForResult()
            {
                _retValTask.Wait();
                return _retValTask.Result;
            }

            internal abstract TRet InvokeFunc();
        }

        private class MarshalItemAction<T> : MarshalItem
        {
            private readonly Action<T> _action;
            private readonly T _arg1;

            internal MarshalItemAction(Action<T> action, T arg1)
            {
                _action = action;
                _arg1 = arg1;
            }

            internal override void Invoke()
            {
                _action(_arg1);
            }
        }

        private class MarshalItemAction<T1, T2> : MarshalItem
        {
            private readonly Action<T1, T2> _action;
            private readonly T1 _arg1;
            private readonly T2 _arg2;

            internal MarshalItemAction(Action<T1, T2> action, T1 arg1, T2 arg2)
            {
                _action = action;
                _arg1 = arg1;
                _arg2 = arg2;
            }

            internal override void Invoke()
            {
                _action(_arg1, _arg2);
            }
        }

        private class MarshalItemFunc<TRet> : MarshalItemFuncBase<TRet>
        {
            private readonly Func<TRet> _func;

            internal MarshalItemFunc(Func<TRet> func)
            {
                _func = func;
            }

            internal override TRet InvokeFunc()
            {
                return _func();
            }
        }

        private class MarshalItemFunc<T1, TRet> : MarshalItemFuncBase<TRet>
        {
            private readonly T1 _arg1;
            private readonly Func<T1, TRet> _func;

            internal MarshalItemFunc(Func<T1, TRet> func, T1 arg1)
            {
                _func = func;
                _arg1 = arg1;
            }

            internal override TRet InvokeFunc()
            {
                return _func(_arg1);
            }
        }

        private class MarshalItemFunc<T1, T2, TRet> : MarshalItemFuncBase<TRet>
        {
            private readonly T1 _arg1;
            private readonly T2 _arg2;
            private readonly Func<T1, T2, TRet> _func;

            internal MarshalItemFunc(Func<T1, T2, TRet> func, T1 arg1, T2 arg2)
            {
                _func = func;
                _arg1 = arg1;
                _arg2 = arg2;
            }

            internal override TRet InvokeFunc()
            {
                return _func(_arg1, _arg2);
            }
        }

        private class MarshalItemFunc<T1, T2, T3, TRet> : MarshalItemFuncBase<TRet>
        {
            private readonly T1 _arg1;
            private readonly T2 _arg2;
            private readonly T3 _arg3;
            private readonly Func<T1, T2, T3, TRet> _func;

            internal MarshalItemFunc(Func<T1, T2, T3, TRet> func, T1 arg1, T2 arg2, T3 arg3)
            {
                _func = func;
                _arg1 = arg1;
                _arg2 = arg2;
                _arg3 = arg3;
            }

            internal override TRet InvokeFunc()
            {
                return _func(_arg1, _arg2, _arg3);
            }
        }

        private class MarshalItemFuncOut<T1, T2, T3, TRet, TOut> : MarshalItem
        {
            private readonly T1 _arg1;
            private readonly T2 _arg2;
            private readonly T3 _arg3;
            private readonly FuncOut _func;
            private readonly Task<TRet> _retValTask;
            private TOut _outVal;

            private TRet _retVal;

            internal MarshalItemFuncOut(FuncOut func, T1 arg1, T2 arg2, T3 arg3)
            {
                _func = func;
                _arg1 = arg1;
                _arg2 = arg2;
                _arg3 = arg3;
                _retValTask = new Task<TRet>(() => _retVal);
            }

            internal override void Invoke()
            {
                _retVal = _func(_arg1, _arg2, _arg3, out _outVal);
                _retValTask.Start();
            }

            internal TRet WaitForResult(out TOut val)
            {
                _retValTask.Wait();
                val = _outVal;
                return _retValTask.Result;
            }

            internal delegate TRet FuncOut(T1 t1, T2 t2, T3 t3, out TOut tout);
        }

        private class MarshalItemFuncRef<T1, T2, TRet, TRef1, TRef2> : MarshalItem
        {
            private readonly T1 _arg1;
            private readonly T2 _arg2;
            private readonly FuncRef _func;

            private readonly Task<TRet> _retValTask;
            private TRef1 _arg3;
            private TRef2 _arg4;
            private TRet _retVal;

            internal MarshalItemFuncRef(FuncRef func, T1 arg1, T2 arg2, TRef1 arg3, TRef2 arg4)
            {
                _func = func;
                _arg1 = arg1;
                _arg2 = arg2;
                _arg3 = arg3;
                _arg4 = arg4;
                _retValTask = new Task<TRet>(() => _retVal);
            }

            internal override void Invoke()
            {
                _retVal = _func(_arg1, _arg2, ref _arg3, ref _arg4);
                _retValTask.Start();
            }

            // ReSharper disable RedundantAssignment
            internal TRet WaitForResult(ref TRef1 ref1, ref TRef2 ref2)
            {
                _retValTask.Wait();
                ref1 = _arg3;
                ref2 = _arg4;
                return _retValTask.Result;
            }
            // ReSharper restore RedundantAssignment

            internal delegate TRet FuncRef(T1 t1, T2 t2, ref TRef1 tref1, ref TRef2 tref2);
        }

        #endregion items
    }
}