using Oxide.Core;
using System;
using System.Threading;
using VRage.Utils;
using Logger = Oxide.Core.Logging.Logger;

namespace Oxide.Game.SpaceEngineers
{
    /// <summary>
    /// A logger that writes to the Unity console
    /// </summary>
    public sealed class SpaceEngineersLogger : Logger
    {
        private readonly Thread mainThread = Thread.CurrentThread;

        /// <summary>
        /// Initializes a new instance of the UnityLogger class
        /// </summary>
        public SpaceEngineersLogger() : base(true)
        {
        }

        /// <summary>
        /// Immediately writes a message to the unity console
        /// </summary>
        /// <param name="message"></param>
        protected override void ProcessMessage(LogMessage message)
        {
            if (Thread.CurrentThread != mainThread)
            {
                Interface.Oxide.NextTick(() => ProcessMessage(message));
                return;
            }

            try
            {
                if (MyLog.Default != null && MyLog.Default.LogEnabled)
                    MyLog.Default.WriteLineAndConsole(message.Type.ToString() + ": " + message.ConsoleMessage);
            }
            catch
            {
            }

        }

    }
}
