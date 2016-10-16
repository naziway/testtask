using Heathmill.FixAT.Server.Commands;

namespace Heathmill.FixAT.Server
{
    internal interface ICommandQueue
    {
        /// <summary>
        ///     Adds a command to the queue
        /// </summary>
        void Enqueue(ICommand command);

        /// <summary>
        ///     Gets the first command in the queue
        /// </summary>
        /// <returns>The command, or null if the queue is empty</returns>
        ICommand Dequeue();

        /// <summary>
        ///     Clears the queue
        /// </summary>
        void Clear();
    }
}