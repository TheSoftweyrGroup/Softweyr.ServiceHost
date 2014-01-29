/*
namespace Softweyr.Infrastructure.Wcf
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Web.Configuration;

    /// <summary>
    /// Utility class for assisting with common WCF actions.
    /// </summary>
    public static class WcfHelper
    {
        /// <summary>
        /// The configuration section for WCF.
        /// </summary>
        private static readonly ServiceModelSectionGroup ServiceModelConfiguration;

        /// <summary>
        /// Initializes static members of the <see cref="WcfHelper"/> class.
        /// </summary>
        static WcfHelper()
        {
            if (System.Web.HttpContext.Current != null)
            {
                ServiceModelConfiguration = ServiceModelSectionGroup.GetSectionGroup(
                    WebConfigurationManager.OpenWebConfiguration("~/"));
            }
            else
            {
                ServiceModelConfiguration = ServiceModelSectionGroup.GetSectionGroup(
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            }
        }

        /// <summary>
        /// Invokes a remote WCF service method while avoid memory leaks and closing channels correctly.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="TServiceContract">
        /// The type of service to which the message is going.
        /// </typeparam>
        public static void Invoke<TServiceContract>(Action<TServiceContract> action)
        {
            for (var i = 0; i < ServiceModelConfiguration.Client.Endpoints.Count; i++)
            {
                var endpointConfig = ServiceModelConfiguration.Client.Endpoints[i];
                if (typeof(TServiceContract).Assembly.GetType(endpointConfig.Contract) == typeof(TServiceContract))
                {
                    var channelFactory = new ChannelFactory<TServiceContract>(endpointConfig.Name);
                    channelFactory.Open();
                    try
                    {
                        using (var channel = (IDisposable)channelFactory.CreateChannel())
                        {
                            try
                            {
                                action.Invoke((TServiceContract)channel);
                                ((IChannel)channel).Close();
                                return;
                            }
                            catch (TimeoutException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (CommunicationException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (Exception exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            channelFactory.Close();
                        }
                        catch (Exception exception)
                        {
                            // TODO: Reenable logging.
                            // Log.Error(exception);
                        }
                    }
                }
            }

            throw new Exception(string.Format("No client endpoint configured for service contract '{0}'", (typeof(TServiceContract).Name)));
        }

        /// <summary>
        /// Invokes a remote WCF service method while avoid memory leaks and closing channels correctly.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="endpointConfigurationName">
        /// The endpoint configuration name.
        /// </param>
        /// <typeparam name="TServiceContract">
        /// The type of service to which the message is going.
        /// </typeparam>
        public static void Invoke<TServiceContract>(Action<TServiceContract> action, string endpointConfigurationName)
        {
            var channelFactory = new ChannelFactory<TServiceContract>(endpointConfigurationName);
            channelFactory.Open();
            try
            {
                using (var channel = (IDisposable)channelFactory.CreateChannel())
                {
                    try
                    {
                        action.Invoke((TServiceContract)channel);
                        ((IChannel)channel).Close();
                        return;
                    }
                    catch (TimeoutException exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                    catch (CommunicationException exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                    catch (Exception exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                }
            }
            finally
            {
                try
                {
                    channelFactory.Close();
                }
                catch (Exception exception)
                {
                    // TODO: Reenable logging.
                    // Log.Error(exception);
                }
            }
        }

        /// <summary>
        /// Invokes a remote WCF service method while avoid memory leaks and closing channels correctly.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="endpointConfigurationName">
        /// The endpoint configuration name.
        /// </param>
        /// <param name="endpointAddress">
        /// The endpoint Address.
        /// </param>
        /// <typeparam name="TServiceContract">
        /// The type of service to which the message is going.
        /// </typeparam>
        public static void Invoke<TServiceContract>(Action<TServiceContract> action, string endpointConfigurationName, EndpointAddress endpointAddress)
        {
            var channelFactory = new ChannelFactory<TServiceContract>(endpointConfigurationName);
            channelFactory.Open();
            try
            {
                using (var channel = (IDisposable)channelFactory.CreateChannel(endpointAddress))
                {
                    try
                    {
                        action.Invoke((TServiceContract)channel);
                        ((IChannel)channel).Close();
                        return;
                    }
                    catch (TimeoutException exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                    catch (CommunicationException exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                    catch (Exception exception)
                    {
                        // TODO: Reenable logging.
                        // Log.Error(exception);
                        ((IClientChannel)channel).Abort();
                        throw;
                    }
                }
            }
            finally
            {
                try
                {
                    channelFactory.Close();
                }
                catch (Exception exception)
                {
                    // TODO: Reenable logging.
                    // Log.Error(exception);
                }
            }
        }

        /// <summary>
        /// Makes a request to a remote WCF service function expecting a response while avoid memory leaks and closing channels correctly.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="TServiceContract">
        /// The type of service to which the message is going.
        /// </typeparam>
        /// <typeparam name="TReturnType">
        /// The return type of the request.
        /// </typeparam>
        /// <returns>
        /// The result of the invokation.
        /// </returns>
        public static TReturnType Request<TServiceContract, TReturnType>(Func<TServiceContract, TReturnType> action)
        {
            for (var i = 0; i < ServiceModelConfiguration.Client.Endpoints.Count; i++)
            {
                var endpointConfig = ServiceModelConfiguration.Client.Endpoints[i];
                if (typeof(TServiceContract).Assembly.GetType(endpointConfig.Contract) == typeof(TServiceContract))
                {
                    var channelFactory = new ChannelFactory<TServiceContract>(endpointConfig.Name);
                    channelFactory.Open();
                    try
                    {
                        using (var channel = (IDisposable)channelFactory.CreateChannel())
                        {
                            try
                            {
                                var retVal = action.Invoke((TServiceContract)channel);
                                try
                                {
                                    return retVal;
                                }
                                finally
                                {
                                    ((IChannel)channel).Close();
                                }
                            }
                            catch (TimeoutException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (CommunicationException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (Exception exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            channelFactory.Close();
                        }
                        catch (Exception exception)
                        {
                            // TODO: Reenable logging.
                            // Log.Error(exception);
                        }
                    }
                }
            }

            throw new Exception(string.Format("No client endpoint configured for service contract '{0}'", (typeof(TServiceContract).Name)));
        }

        /// <summary>
        /// Publishes a message to all WCF client endpoints in the configuration file for the specified contract.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="TServiceContract">
        /// The type of service to which the message is going.
        /// </typeparam>
        public static void Publish<TServiceContract>(Action<TServiceContract> action)
        {
            var matches = 0;
            for (var i = 0; i < ServiceModelConfiguration.Client.Endpoints.Count; i++)
            {
                var endpointConfig = ServiceModelConfiguration.Client.Endpoints[i];
                if (typeof(TServiceContract).Assembly.GetType(endpointConfig.Contract) == typeof(TServiceContract))
                {
                    matches++;
                    var channelFactory = new ChannelFactory<TServiceContract>(endpointConfig.Name);
                    channelFactory.Open();
                    try
                    {
                        using (var channel = (IDisposable)channelFactory.CreateChannel())
                        {
                            try
                            {
                                action.Invoke((TServiceContract)channel);
                                ((IChannel)channel).Close();
                            }
                            catch (TimeoutException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (CommunicationException exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                            catch (Exception exception)
                            {
                                // TODO: Reenable logging.
                                // Log.Error(exception);
                                ((IClientChannel)channel).Abort();
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            channelFactory.Close();
                        }
                        catch (Exception exception)
                        {
                            // TODO: Reenable logging.
                            // Log.Error(exception);
                        }
                    }
                }
            }

            if (matches == 0)
            {
                throw new Exception(string.Format("No client endpoint configured for service contract '{0}'", typeof(TServiceContract).Name));
            }
        }
    }
}
*/