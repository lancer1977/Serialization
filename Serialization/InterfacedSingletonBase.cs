using System.Diagnostics;

namespace PolyhydraGames.Core.Serialization
{
    [DebuggerStepThrough]
    public abstract class InterfacedSingletonBase<T, TI> where T : TI, new() where TI : class
    {
        /// <summary>
        /// Concurrency Locking Object.
        /// </summary>
        protected static readonly object _ConcurrencyLock = new object();

        /// <summary>
        /// Holds the current active singleton object.
        /// </summary>
        private static volatile TI _Current;

        /// <summary>
        /// Gets or sets the Singleton Current Static Property.
        /// </summary>
        public static TI Current
        {
            get
            {
                if (_Current == null)
                {
                    lock (_ConcurrencyLock)
                    {
                        if (_Current == null)
                        {
                            _Current = new T();
                        }
                    }
                }

                return _Current;
            }
            set
            {
                lock (_ConcurrencyLock)
                {
                    _Current = value;
                }
            }
        }
    }
}
