using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using WcfApplication.Common;

namespace WcfApplication.Examples.Duplex
{
    public class BasicHttpCallback : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "FaultException. Simple methods calls.";
            }
        }

        public override void Execute(UserSettings settings)
        {
            if(!(settings.Binding is BasicHttpBinding))
            {
                SysConsole.WriteErrorLine($"Only {nameof(BasicHttpBinding)} supports");
                return;
            }

            switch (settings.AppSide)
            {
                case AppSide.Client:
                    break;
                case AppSide.Server:
                    break;
            }
        }
    }
}
