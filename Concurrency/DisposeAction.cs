using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Concurrency
{
    /// <summary>
    /// This class can be used to provide an action when
    /// Dipose method is called.
    /// </summary>
    public class DisposeAction : IDisposable
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a new <see cref="DisposeAction"/> object.
        /// </summary>
        /// <param name="action">Action to be executed when this object is disposed.</param>
        public DisposeAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        public void Dispose()
        {
            StackTrace stack = new StackTrace();
            Console.WriteLine("caller: " + stack.GetFrame(1).GetMethod().Name);
            _action();
        }
    }
}
