namespace Softweyr.Infrastructure.Wcf
{
    using System;
    using System.Linq;
    using System.ServiceModel;

    /// <summary>
    /// A service host that will inject dependencies using the inversion of control container.
    /// </summary>
    public class IocServiceHost : ServiceHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IocServiceHost"/> class.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        /// <param name="baseAddresses">
        /// The base addresses.
        /// </param>
        private IocServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IocServiceHost"/> class.
        /// </summary>
        /// <param name="singleton">
        /// The singleton.
        /// </param>
        /// <param name="baseAddresses">
        /// The base addresses.
        /// </param>
        private IocServiceHost(object singleton, params Uri[] baseAddresses)
            : base(singleton, baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of IocServiceHost based on the given service type.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        /// <param name="baseAddresses">
        /// The base addresses.
        /// </param>
        /// <returns>
        /// A new instance of IocServiceHost based on the given parameters.
        /// </returns>
        public static IocServiceHost CreateServiceHost(Type serviceType, params Uri[] baseAddresses)
        {
            var behaviorAttribute = (ServiceBehaviorAttribute)serviceType.GetCustomAttributes(typeof(ServiceBehaviorAttribute), true).SingleOrDefault();
            if (behaviorAttribute != null && behaviorAttribute.InstanceContextMode == InstanceContextMode.Single)
            {
                var singleton = Ioc.Resolve(serviceType);
                var serviceHost = new IocServiceHost(singleton, baseAddresses);

                return serviceHost;
            }

            return new IocServiceHost(serviceType, baseAddresses);
        }

        /// <summary>
        /// Invoked during the transition of a communication object into the opening state.
        /// </summary>
        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new PoisonErrorBehaviorAttribute(typeof(PoisonErrorHandler)));
            var behavior = new IocServiceBehaviorAttribute();
            this.Description.Behaviors.Add(behavior);
            base.OnOpening();
        }
    }
}