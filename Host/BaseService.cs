using Softweyr.Logging;
using Softweyr.Infrastructure.Wcf;

namespace Softweyr.Infrastructure.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceProcess;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;

    using Microsoft.Win32;

    /// <summary>
    /// An implementation of BaseService that uses the default ServiceControllerForm.
    /// </summary>
    public abstract class BaseService<TThis> : BaseService<TThis, ServiceControllerForm>
        where TThis : BaseService<TThis, ServiceControllerForm>, new()
    {
    }

    /// <summary>
    /// The base service from which all Softweyr service hosts should inherit from.
    /// </summary>
    /// <typeparam name="TWinForm">
    /// The type of windows form to show when the showwindow argument is supplied.
    /// </typeparam>
    // [InstallerType(typeof(ServiceProcessInstaller))]
    public abstract class BaseService<TThis, TWinForm> : ServiceBase
        where TThis : BaseService<TThis, TWinForm>, new()
        where TWinForm : Form, new()
    {
        /// <summary>
        /// The service hosts being control by this service.
        /// </summary>
        private List<ServiceHost> serviceHosts = new List<ServiceHost>();

        /// <summary>
        /// The services being hosted by this service.
        /// </summary>
        private List<Type> services = new List<Type>();

        /// <summary>
        /// The start action to take before starting any services.
        /// </summary>
        private Action preStartAction;

        /// <summary>
        /// The start action to take before starting any services.
        /// </summary>
        private Action postStartAction;

        /// <summary>
        /// The stop actions to take after stopping all the services.
        /// </summary>
        private Action preStopAction;

        /// <summary>
        /// The stop actions to take after stopping all the services.
        /// </summary>
        private Action postStopAction;

        /// <summary>
        /// The windows form currently running.
        /// </summary>
        private Form windowsForm;

        /// <summary>
        /// The UI thread.
        /// </summary>
        private System.Threading.Thread windowThread;

        /// <summary>
        /// List of custom arguments for the current Base Service implementation.
        /// </summary>
        private readonly Dictionary<string, string> customArguments = new Dictionary<string, string>();

        /// <summary>
        /// Gets the human readable name to be displayed to identify this service.
        /// </summary>
        protected virtual string DisplayName { get { return string.Format("{0} {1}", this.GetType().FullName, this.GetType().Assembly.GetName().Version); } }

// Supress new keyword.
#pragma warning disable 108,114 
        /// <summary>
        /// Gets the service name to use when installing this service as a windows service.
        /// </summary>
        protected virtual string ServiceName { get { return EnsureSafeServiceName(this.DisplayName); } }
#pragma warning restore 108,114

        public static string EnsureSafeServiceName(string unsafeServiceName)
        {
            return unsafeServiceName.Replace(".", "_").Replace(" ", "_");
        }

        public static void Main(params string[] args)
        {
            ((BaseService<TThis, TWinForm>) new TThis()).RunWithArgs(args);
        }

        /// <summary>
        /// Execute the service with the given arguments, using a <see cref="IocServiceHost"/> for each of the given WCF service types, use the given startup action and stop action.
        /// </summary>
        /// <param name="bootstrap">
        /// The bootstrap sequence that takes the command line arguments to execute to initialize the domain.
        /// </param>
        /// <param name="args">
        /// The arguments to use.
        /// </param>
        /// <param name="services">
        /// The WCF service types that this service will host.
        /// </param>
        /// <param name="preStartAction">
        /// The pre Start Action.
        /// </param>
        /// <param name="postStartAction">
        /// The post Start Action.
        /// </param>
        /// <param name="preStopAction">
        /// The pre Stop Action.
        /// </param>
        /// <param name="postStopAction">
        /// The post Stop Action.
        /// </param>
        protected void RunWithArgs(string[] args, Action<string[]> bootstrap = null, string displayName = null, string serviceName = null, List<Type> services = null, Action preStartAction = null, Action postStartAction = null, Action preStopAction = null, Action postStopAction = null, string helpText = null)
//// ReSharper restore ParameterHidesMember
        {
            try
            {
                // Set up monitoring.
                this.services = services ?? new List<Type>();

                this.preStartAction = preStartAction;
                this.postStartAction = postStartAction;
                this.preStopAction = preStopAction;
                this.postStopAction = postStopAction;

                var options = CommandLineOptions.FromArgs(args, displayName ?? this.DisplayName, this.ServiceName);

                if (options.ConfigFile != string.Empty && !options.Install && !options.Uninstall)
                {
                    // if the config has been overriden then execute this same process using the specified config file.
                    ExecuteUsingConfig(args, options.ConfigFile);
                    return;
                }

                if (options.Version)
                {
                    Console.WriteLine(this.GetType().Assembly.GetName().Version);
                }

                if (bootstrap != null)
                {
                    bootstrap.Invoke(args);
                }

                if (options.Uninstall)
                {
                    this.ManualUninstall(options.ServiceName);
                    if (!options.Install)
                    {
                        return;
                    }
                }

                if (options.Install)
                {
                    this.CheckDependenciesAreInstalled();
                    this.ManualInstall(options.ServiceName, options.DisplayName, string.Join(" ", args.Where(arg => arg.ToLower() != "-service" && arg.ToLower() != "-install" && !CommandLineOptions.StartsWithPrefixes(CommandLineOptions.ServiceNameOverrideArgumentPrefixes, arg) && !CommandLineOptions.StartsWithPrefixes(CommandLineOptions.DisplayNameOverrideArgumentPrefixes, arg))));
                    return;
                }

                /* bootstrap */
                /*
                if (trace)
                {
                    // TODO: Re-add logging.
                    Log.SetLevel(LoggingLevel.Trace);
                }
                else
                {
                    // TODO: Re-add logging.
                    Log.Debug("Fetching system profile from registry.");

                    string profile;
                    using (RegistryKey profileKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Softweyr\Host"))
                    {
                        if (profileKey == null)
                        {
                            throw new InvalidOperationException(
                                @"Missing registry key HKLM\SOFTWARE\Softweyr\Host\Profile");
                        }

                        profile = profileKey.GetValue("Profile", null, RegistryValueOptions.None) as string;
                        if (profile == null)
                        {
                            throw new InvalidOperationException(
                                @"Missing/Invalid registry key HKLM\SOFTWARE\Softweyr\Host\Profile");
                        }

                        profileKey.Close();
                    }

                    switch (profile.ToLower())
                    {
                        case "Softweyr.development":
                            // TODO: Re-add logging.
                            Log.SetLevel(LoggingLevel.Debug);
                            break;
                        case "Softweyr.staging":
                            // TODO: Re-add logging.
                            Log.SetLevel(LoggingLevel.Information);
                            break;
                        case "Softweyr.production":
                            // TODO: Re-add logging.
                            Log.SetLevel(LoggingLevel.Warning);
                            break;
                        default:
                            throw new ApplicationException(string.Format("Unknown recognised profile {0}.", profile));
                    }
                } */

                // TODO: Re-add logging.
                // Log.SetApplicationName(this.ServiceName);

                if (options.RunAsService && !options.RunAsConsole)
                {
                    // Run as a service.
                    // TODO: Re-add logging.
                    Log.Information<TThis>("Starting Services as a Windows Service.");
                    Run(this);
                }
                else if (options.RunAsConsole)
                {
                    // Run as a console app.
                    // TODO: Re-add logging.
                    Log.Information<TThis>("Starting Services as a Console Application.");
                    this.Start();
                    if (options.ShowWindow)
                    {
                        this.windowsForm = new TWinForm();
                        this.OpenWindow();
                    }

                    // TODO: Re-add logging.
                    Log.Information<TThis>("Service started. Press <Enter> to stop listening.");
                    Console.ReadLine();
                    this.Stop();
                    if (options.ShowWindow)
                    {
                        this.CloseWindow();
                    }
                }
                else if (options.RunAsService && options.RunAsConsole)
                {
                    throw new Exception("-console and -service flags are mutually exclusive. Only set one of these flags");
                }
                else if (!options.Version)
                {
                    if (!string.IsNullOrWhiteSpace(helpText))
                    {
                        Console.WriteLine(helpText);
                    }
                    else
                    {
                        Console.WriteLine(
     @"
*** {0} Service Host Application ***

Generic Wcf Service Host Application for hosting the following services,

{1}

The following command line arguments can be specified,

  -console                     Run application as a console application.
  -service                     Run application as a windows service.
  -version                     Prints the version of the application assembly.
  -install                     Install as a windows service.
  -uninstall                   Uninstall as a windows service.
  -showwindow                  Shows a service management window while running as
                               a console application.
  -config=<PathToConfigFile>   Use the given application config file inplace of the
                               configuration file for this executable.
  -cfg=<PathToConfigFile>      Same as above.
  -servicename=<ServiceName>   Overrides the service name when installing as a
                               windows service.
  -sname=<ServiceName>         Same as above.
  -displayname=<DisplayName>   Overrides the display name when installing as a
                               windows service.
  -dname=<DisplayName>         Same as above.
", this.GetType().Assembly.GetName().Name, this.services.Any() ? string.Join(Environment.NewLine, this.services.Select(svc => svc.FullName).ToArray()) : "No services specified.");
                    }
                }

                if (options.RunAsConsole || options.RunAsService)
                {
                    Log.Information<TThis>("Service Terminated.");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal<TThis>(ex.Message, ex);
            }
        }

        /// <summary>
        /// Adds a custom argument to the list of allowed arguments.
        /// </summary>
        /// <param name="argument">
        /// The argument.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        protected void AddCustomArgument(string argument, string description)
        {
            this.customArguments.Add(argument.ToLower(), description);
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void CloseWindow()
        {
            this.windowsForm.Invoke((Action)(() => this.windowsForm.Close()));
            this.windowThread.Join();
        }

        /// <summary>
        /// Opens the window on a new thread.
        /// </summary>
        private void OpenWindow()
        {
            this.windowThread = new System.Threading.Thread(this.WindowApplicationBackgroundWorker);
            this.windowThread.Start();
        }

        private void WindowApplicationBackgroundWorker()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(this.windowsForm);
        }

        private void CheckDependenciesAreInstalled()
        {
            // TODO: Do this without using ServiceHost (otherwise an instance of the service might start).
            /*
            foreach (Type serviceType in this.services)
            {
                var serviceHost = new IocServiceHost(serviceType);
                var messageQueuePaths = serviceHost.Description.Endpoints.Where(ep => ep.Binding is NetMsmqBinding).Select(ep => GetQueuePath(ep.Address));
                foreach (var messageQueuePath in messageQueuePaths)
                {
                    if (!MessageQueue.Exists(messageQueuePath))
                    {
                        using (var mq = MessageQueue.Create(messageQueuePath, true))
                        {
                            mq.Close();
                        }

                        Log.Debug("'{0}' Queue Added for {1} service.".FormatWith(messageQueuePath, serviceType.Name));
                    }
                }
            }  */
        }

        private static string GetQueuePath(EndpointAddress address)
        {
            return string.Join(
                "\\",
                new[] { "." }.Union(
                    address.Uri.Segments.Skip(1).Select(
                        seg => seg.EndsWith("/") ? seg.Substring(0, seg.Length - 1) : seg).Select(
                            seg => seg.ToLower() == "private" ? "Private$" : seg)).ToArray());
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            this.Stop();
            base.OnStop();
        }

        public void Start()
        {
            if (this.preStartAction != null)
            {
                this.preStartAction();
            }

            // TODO: Re-add logging.
            Log.Debug("Starting {0} Service.".FormatWith(this.DisplayName));
            foreach (Type serviceType in services)
            {
                try
                {
                    var serviceHost = IocServiceHost.CreateServiceHost(serviceType);
                    var messageQueuePaths = serviceHost.Description.Endpoints.Where(ep => ep.Binding is NetMsmqBinding).Select(ep => GetQueuePath(ep.Address));
                    foreach (var messageQueuePath in messageQueuePaths)
                    {
                        if (!MessageQueue.Exists(messageQueuePath))
                        {
                            using (var mq = MessageQueue.Create(messageQueuePath, true))
                            {
                                mq.Close();
                            }

                            // TODO: Re-add logging.
                            Log.Debug("'{0}' Queue Added for {1} service.".FormatWith(messageQueuePath, serviceType.Name));
                        }
                    }

                    serviceHost.Faulted += this.ServiceHostFaultedEventHandler;
                    serviceHost.UnknownMessageReceived += this.ServiceHostUnknownMessageReceivedEventHandler;
                    serviceHost.Open();
                    this.serviceHosts.Add(serviceHost);
                    // TODO: Re-add logging.
                    Log.Debug(serviceType.Name + " Service Started.");
                }
                catch (Exception ex)
                {
                    // TODO: Re-add logging.
                    Log.Error<TThis>("Exception while starting service {0} :: {1}".FormatWith(serviceType.Name, ex.Message), ex);
                    throw;
                }
            }

            if (this.postStartAction != null)
            {
                this.postStartAction();
            }
        }

        /// <summary>
        /// Handles the UnknownMessageReceived event on a service host.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ServiceHostUnknownMessageReceivedEventHandler(object sender, UnknownMessageReceivedEventArgs e)
        {
            var messageReader = e.Message.GetReaderAtBodyContents();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(messageReader);
            // TODO: Re-add logging.
            Log.Error<TThis>("Unknown message received :: " + xmlDoc);
        }

        /// <summary>
        /// Handles the FaultedEvent on a service host.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ServiceHostFaultedEventHandler(object sender, EventArgs e)
        {
            var serviceHost = sender as IocServiceHost;
            if (serviceHost == null)
            {
                // TODO: Re-add logging.
                Log.Error<TThis>("Failed to restart faulted service due to unknown faulted event sender.");
                throw new ApplicationException("Failed to restart faulted service due to unknown faulted event sender.");
            }

            if (serviceHost.Description == null || serviceHost.Description.ServiceType == null)
            {
                // TODO: Re-add logging.
                Log.Error<TThis>("Failed to restart faulted service as unable to discover unknown service type.");
                throw new ApplicationException(
                    "Failed to restart faulted service as unable to discover unknown service type.");
            }

            var newServiceThread = new Thread(
                () =>
                    {
                        try
                        {
                            // TODO: Create a better retry mechanism.
                            Log.Warning<TThis>("Service Host for {0} service has faulted. Auto-restarting service host in 10 seconds..".FormatWith(serviceHost.Description.ServiceType));
                            Thread.Sleep(10000);
                            serviceHost.Abort();
                            serviceHost.Faulted -= this.ServiceHostFaultedEventHandler;
                            serviceHost.UnknownMessageReceived -= this.ServiceHostUnknownMessageReceivedEventHandler;
                            this.serviceHosts.Remove(serviceHost);
                            Log.Warning<TThis>("Faulted Service Host for {0} service has aborted current operations. Starting new service host.".FormatWith(serviceHost.Description.ServiceType));
                            serviceHost = IocServiceHost.CreateServiceHost(serviceHost.Description.ServiceType);
                            this.serviceHosts.Add(serviceHost);
                            serviceHost.Faulted += this.ServiceHostFaultedEventHandler;
                            serviceHost.UnknownMessageReceived += this.ServiceHostUnknownMessageReceivedEventHandler;
                            serviceHost.Open();
                            Log.Warning<TThis>("New Service Host for {0} service has started successfully.".FormatWith(serviceHost.Description.ServiceType));
                        }
                        catch (Exception ex)
                        {
                            Log.Error<TThis>("Exception while attempting to restart service host for {0}".FormatWith(serviceHost.Description.ServiceType), ex);
                            throw;
                        }
                    });

            newServiceThread.Start();
        }

        public new void Stop()
        {
            if (this.preStopAction != null)
            {
                this.preStopAction();
            }

            var resetEvents = new List<ManualResetEvent>();

            foreach (var serviceHost in this.serviceHosts.ToArray())
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);
                // TODO: Re-add logging.
                Log.Debug("Stopping Service.");
                serviceHost.Closed += (object sender, EventArgs args) => resetEvent.Set();
                serviceHost.Faulted -= this.ServiceHostFaultedEventHandler;
                serviceHost.UnknownMessageReceived -= this.ServiceHostUnknownMessageReceivedEventHandler;
                serviceHost.Close();
                // TODO: Re-add logging.
                Log.Debug("Service Stopped.");
                this.serviceHosts.Remove(serviceHost);
            }

            foreach (var resetEvent in resetEvents)
            {
                resetEvent.WaitOne();
                resetEvent.Close();
                resetEvent.Dispose();
            }

            if (this.postStopAction != null)
            {
                this.postStopAction();
            }
        }

        private void ManualInstall(string serviceName, string displayName, string args)
        {
            // Install the service
            var installProc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "sc.exe",
                    Arguments =
                        "create " + serviceName + " binPath= \"" +
                        Assembly.GetEntryAssembly().Location + " " + args +
                        " -service\" DisplayName= \"" + (displayName ?? serviceName) + "\""
                }
            };
            installProc.Start();
            Console.Write(installProc.StandardOutput.ReadToEnd());
            installProc.WaitForExit(30000);
        }

        /// <summary>
        /// Manually uninstalls a service.
        /// </summary>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        private void ManualUninstall(string serviceName)
        {
            // Uninstall the service
            var stopProc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "sc.exe",
                    Arguments =
                        "stop " + serviceName
                }
            };
            stopProc.Start();
            Console.Write(stopProc.StandardOutput.ReadToEnd());
            stopProc.WaitForExit(30000);

            // Uninstall the service
            var uninstallProc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "sc.exe",
                    Arguments =
                        "delete " + serviceName
                }
            };
            uninstallProc.Start();
            Console.Write(uninstallProc.StandardOutput.ReadToEnd());
            uninstallProc.WaitForExit(30000);
        }

        private static void ExecuteUsingConfig(string[] originalArgs, string configFile)
        {
            // Get host domain settings.
            var hostConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile.Replace(".vshost", string.Empty);
            var hostAppName = AppDomain.CurrentDomain.SetupInformation.ApplicationName.Replace(".vshost", string.Empty);
            var hostEntryAssembly = Assembly.GetEntryAssembly().GetName();
            var hostDomainName = AppDomain.CurrentDomain.FriendlyName;

            // Calculate instance domain settings.
            string instanceConfigFilePath;
            if (!Path.IsPathRooted(configFile))
            {
                instanceConfigFilePath = Path.Combine(Path.GetDirectoryName(hostConfigFile), configFile);
            }
            else
            {
                instanceConfigFilePath = configFile;
            }

            var instanceConfigFileName = Path.GetFileName(instanceConfigFilePath);
            
            if (!File.Exists(instanceConfigFilePath))
            {
                throw new ApplicationException(string.Format("File \"{0}\" does not exists.", instanceConfigFilePath));
            }

            var instanceAppName = string.Format("{0}.{1}", hostAppName, instanceConfigFileName);
            var instanceDomainName = string.Format("{0} using {1}", hostDomainName, instanceConfigFileName);

            // Create domain
            var setup = new AppDomainSetup
            {
                ApplicationName = instanceAppName,
                ConfigurationFile = instanceConfigFilePath
            };
            var domain = AppDomain.CreateDomain(instanceDomainName, null, setup);

            var instanceIdFreeArgs = originalArgs.Where(arg => !CommandLineOptions.StartsWithPrefixes(CommandLineOptions.ConfigOverrideArgumentPrefixes, arg)).ToArray();
            domain.ExecuteAssemblyByName(hostEntryAssembly, instanceIdFreeArgs);
        }
    }

    internal class CommandLineOptions
    {
        private CommandLineOptions(string defaultDisplayName, string defaultServiceName)
        {
            this.DisplayName = defaultDisplayName;
            this.ServiceName = defaultServiceName;
            this.ConfigFile = string.Empty;
            this.Install = false;
            this.Uninstall = false;
            this.RunAsService = false;
            this.RunAsConsole = false;
            this.Trace = false;
            this.ShowWindow = false;
            this.Version = false;
        }

        /// <summary>
        /// The argument prefix for overriding the config file used.
        /// </summary>
        public static readonly string[] ConfigOverrideArgumentPrefixes = new[] { "-config=", "-cfg=" };

        /// <summary>
        /// The argument prefix for overriding the service display name used.
        /// </summary>
        public static readonly string[] DisplayNameOverrideArgumentPrefixes = new[] { "-displayname=", "-dname=" };

        /// <summary>
        /// The argument prefix for overriding the service name used.
        /// </summary>
        public static readonly string[] ServiceNameOverrideArgumentPrefixes = new[] { "-servicename=", "-sname=" };

        public string DisplayName { get; private set; }
        public string ServiceName { get; private set; }
        public string ConfigFile { get; private set; }
        public bool Install { get; private set; }
        public bool Uninstall { get; private set; }
        public bool RunAsService { get; private set; }
        public bool RunAsConsole { get; private set; }
        public bool Trace { get; private set; }
        public bool ShowWindow { get; private set; }
        public bool Version { get; private set; }

        public static bool StartsWithPrefixes(string[] prefixes, string arg)
        {
            return prefixes.Any(prefix => arg.ToLower().StartsWith(prefix));
        }

        public static string GetValueAfterPrefixes(string[] prefixes, string arg)
        {
            var usedPrefix = prefixes.First(prefix => arg.ToLower().StartsWith(prefix));
            return arg.Substring(usedPrefix.Length);
        }

        public static CommandLineOptions FromArgs(string[] args, string defaultDisplayName, string defaultServiceName)
        {
            var options = new CommandLineOptions(defaultDisplayName, defaultServiceName);

            // Parse arguments
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-install")
                {
                    options.Install = true;
                }
                else if (arg.ToLower() == "-uninstall")
                {
                    options.Uninstall = true;
                }
                else if (arg.ToLower() == "-console")
                {
                    options.RunAsConsole = true;
                }
                else if (arg.ToLower() == "-service")
                {
                    options.RunAsService = true;
                }
                else if (arg.ToLower() == "-version")
                {
                    options.Version = true;
                }
                else if (arg.ToLower() == "-showwindow")
                {
                    options.ShowWindow = true;
                }
                else if (StartsWithPrefixes(DisplayNameOverrideArgumentPrefixes, arg))
                {
                    options.DisplayName = GetValueAfterPrefixes(DisplayNameOverrideArgumentPrefixes, arg);
                }
                else if (StartsWithPrefixes(ServiceNameOverrideArgumentPrefixes, arg))
                {
                    options.ServiceName = GetValueAfterPrefixes(ServiceNameOverrideArgumentPrefixes, arg);
                }
                else if (StartsWithPrefixes(ConfigOverrideArgumentPrefixes, arg))
                {
                    options.ConfigFile = GetValueAfterPrefixes(ConfigOverrideArgumentPrefixes, arg);
                }
                else
                {
                    // Ignore/Warn?
                }
            }

            return options;
        }
    }
}
