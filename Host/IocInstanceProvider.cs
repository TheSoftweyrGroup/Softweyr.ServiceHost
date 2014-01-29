namespace Softweyr.Infrastructure.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// The instance provider that will provide an instance of a particular service type with the dependencies injected using the inversion of control container.
    /// </summary>
    public class IocInstanceProvider : IInstanceProvider
    {
        /// <summary>
        /// The service type this provide provides for.
        /// </summary>
        private readonly Type serviceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocInstanceProvider"/> class.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        public IocInstanceProvider(Type serviceType)
        {
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <returns>
        /// A user-defined service object.
        /// </returns>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <returns>
        /// The service object.
        /// </returns>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param><param name="message">The message that triggered the creation of a service object.</param>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            try
            {
                var instance = Ioc.Resolve(this.serviceType);
                return instance;
            }
            catch (Exception ex)
            {
                // TODO: Reenable logging.
                // Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param><param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }

    public static class Ioc
    {
        public static object Resolve(Type serviceType)
        {
            // TODO: Come up with more loosely coupled method of hooking into this.
            throw new NotImplementedException();
        }
    }
}