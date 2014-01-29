namespace Softweyr.Infrastructure.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// The default handler for poison messages.
    /// </summary>
    public class PoisonErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Enables the creation of a custom <see cref="T:System.ServiceModel.FaultException`1"/> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="T:System.Exception"/> object thrown in the course of the service operation.</param><param name="version">The SOAP version of the message.</param><param name="fault">The <see cref="T:System.ServiceModel.Channels.Message"/> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            // No-op -We are not interested in this. This is only useful if you want to send back a fault on the wire not applicable for queues [one-way].
            // TODO: Reenable logging.
            // Log.Warning(error.GetBaseException());
        }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether subsequent HandleError implementations are called.
        /// </summary>
        /// <returns>
        /// true if subsequent <see cref="T:System.ServiceModel.Dispatcher.IErrorHandler"/> implementations must not be called; otherwise, false. The default is false.
        /// </returns>
        /// <param name="error">The exception thrown during processing.</param>
        public bool HandleError(Exception error)
        {
            if (error != null)
            {
                var exceptionList = new List<string>();
                var lastException = error;
                while (lastException != null)
                {
                    exceptionList.Add(lastException.ToString());
                    lastException = lastException.InnerException;
                }

                if (error.GetType() == typeof(MsmqPoisonMessageException))
                {
                    // TODO: Reenable logging.
                    /*
                    Log.Error(
                        string.Format(
                            " Poisoned message -message look up id = {0} exception = {1}",
                            ((MsmqPoisonMessageException)error).MessageLookupId,
                            string.Join(" :::: ", exceptionList.ToArray())),
                        error.GetBaseException()); */
                }
                else
                {
                    // TODO: Reenable logging.
                    /*
                    Log.Error(
                        string.Format(
                            " Poisoned message exception = {0}",
                            string.Join(" :::: ", exceptionList.ToArray())),
                        error.GetBaseException()); */
                }

                return true;
            }

            return false;
        }
    }
}