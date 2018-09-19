using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using TDCI.BuyDesign.Configurator.Integration.Web;

namespace Infor.Proxy.Controllers
{
    [Route("")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private ConfiguratorSettings _settings;
        private string _serviceUrl;
        public ContentController(IOptions<ConfiguratorSettings> settings)
        {
            _settings = settings.Value;
            _serviceUrl = _settings.Url + "ConfiguratorService/v3/ProductConfigurator.svc"; //TODO: need to sanitize URL to have an ending slash
        }

        [Route("{*relativeUrl}")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(string relativeUrl)
        {
            var hs = new HostServices(_settings.Tenant, _settings.Tenant, _serviceUrl, _settings.ApiKey);
            var filePath = _settings.Url + relativeUrl;
            try
            {
                var content = hs.GetContent(filePath);
                //TODO: consider caching content locally for speed
                return File(content.Blob, GetContentType(content.Filename)); 
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("authenticated"))
                    return Unauthorized();
                return NotFound();
            }
        }


        private static string GetContentType(string fileName)
        {
            string contentType;
            return new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType)
                ? contentType
                : "application/octet-stream";

        }
    }
}
