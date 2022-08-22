using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ImportExportTest.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-and-action-selection

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            //config.Routes.MapHttpRoute(
            //    name: "ActionApi",
            //    routeTemplate: "api/{controller}/{action}",
            //    defaults: null
            //);

            config.Routes.MapHttpRoute(
                name: "ActionIdApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: null
            );

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: null
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ProductCatalogueApi",
                routeTemplate: "api/{controller}/{action}/{clientType}/{segment}/{age}",
                defaults: null
            );
        }
    }
}