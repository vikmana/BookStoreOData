using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
//using System.Web.Http.OData;
using System.Text;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Helpers
{
    public class CustomDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            Assembly assembly = typeof(ODataController).Assembly;
            var thisAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();
            var odatacontrollers = thisAssemblyTypes.Where(t => t.BaseType == typeof(Microsoft.AspNet.OData.ODataController)).ToList();
            var odatamethods = new[] { "Get", "Put", "Post", "Delete" };

            foreach (var odataContoller in odatacontrollers)  // this the OData controllers in the API
            {
                var endpointName = odataContoller.Name.Replace("Controller", "");
                var methods = odataContoller.GetMethods().Where(a => odatamethods.Contains(a.Name)).ToList();

                if (!methods.Any())
                    continue; // next controller 

                foreach (var method in methods)  // this is all of the methods in controller (e.g. GET, POST, PUT, etc)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("/" + method.Name + "(");
                    var listParams = new List<IParameter>();
                    var parameterInfo = method.GetParameters();
                    foreach (ParameterInfo pi in parameterInfo)
                        if (pi.CustomAttributes.Any(a =>
                            a.AttributeType == typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute)))
                        {
                            listParams.Add(new BodyParameter()
                            {
                                Name = pi.Name,
                                Description = pi.ParameterType.ToString(),
                            });
                        }
                        else
                        {
                            listParams.Add(new NonBodyParameter()
                            {
                                Name = pi.Name,
                                Description = pi.ParameterType.ToString(),
                            });
                        }
                    //listParams.Add(pi.ParameterType + " " + pi.Name);

                    //sb.Append(String.Join(", ", listParams.ToArray()));
                    //sb.Append(")");
                    var path = "/" + "api" + "/" + odataContoller.Name.Replace("Controller", "");// + sb.ToString();
                    var odataPathItem = new PathItem();
                    var op = new Operation
                    {
                        Tags = new List<string> { endpointName },
                        OperationId = odataContoller.Name.Replace("Controller", ""),
                        Summary = "Summary about method / data",
                        Description = "Description / options for the call.",
                        Consumes = new List<string>(),
                        Produces = new List<string>
                        {
                            "application/atom+xml",
                            "application/json",
                            "text/json",
                            "application/xml",
                            "text/xml"
                        },
                        Deprecated = false,
                        Parameters = listParams
                    };

                    // The odata methods will be listed under a heading called OData in the swagger doc

                    // This hard-coded for now, set it to use XML comments if you want


                    var response = new Response
                    {
                        Description = "OK",
                        Schema = new Schema
                        {
                            Type = "array",
                            Items = context.SchemaRegistry.GetOrRegister(method.ReturnType)
                        }
                    };
                    op.Responses = new Dictionary<string, Response> { { "200", response } };

                    var security = GetSecurityForOperation(odataContoller);
                    if (security != null)
                        op.Security = new List<IDictionary<string, IEnumerable<string>>> { security };

                    odataPathItem.Get = method.Name.Equals("Get") ? op : odataPathItem.Get;
                    odataPathItem.Post = method.Name.Equals("Post") ? op : odataPathItem.Post;
                    odataPathItem.Put = method.Name.Equals("Put") ? op : odataPathItem.Put;
                    odataPathItem.Delete = method.Name.Equals("Delete") ? op : odataPathItem.Delete;

                    try
                    {
                        swaggerDoc.Paths.Add(path, odataPathItem);
                    }
                    catch { }
                }
            }
        }

        private Dictionary<string, IEnumerable<string>> GetSecurityForOperation(MemberInfo odataContoller)
        {
            Dictionary<string, IEnumerable<string>> securityEntries = null;
            if (odataContoller.GetCustomAttribute(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)) != null)
            {
                securityEntries = new Dictionary<string, IEnumerable<string>> { { "oauth2", new[] { "actioncenter" } } };
            }
            return securityEntries;
        }
    }
}