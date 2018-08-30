using System;
using Umbraco.Core;

namespace Chauffeur.Demo
{
    public class CustomStartup : ApplicationEventHandler
    {
        public CustomStartup()
        {
            UmbracoApplicationBase.ApplicationInit += AppInit;
        }

        private void AppInit(object sender, EventArgs e)
        {
        }
    }
}