namespace SampleHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Softweyr.Infrastructure.Host;

    public class Program : BaseService<Program>
    {
        static void Main(string[] args)
        {
            BaseService<Program>.Main(args);
        }
    }
}
