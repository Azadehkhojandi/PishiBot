using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;

namespace PishiBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            this.RegisterBotModules();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
        private void RegisterBotModules()
        {
            try
            {
                Conversation.UpdateContainer(builder =>
                {
                    builder.RegisterModule<GlobalMessageHandlersBotModule>();
                });
            }
            catch (Exception e)
            {
               //kill exception
            }
           
        }
    }
}
