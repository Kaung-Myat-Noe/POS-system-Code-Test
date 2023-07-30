using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using System.IO;
using System.Net.Http;

namespace apigee.sms.biz.Utilities
{
    public class Cache
    {

        internal static string filepath;
        public static string RetrieveFileContents(string param)
        {
            ObjectCache _cache = MemoryCache.Default;
            CacheItemPolicy _policy = new CacheItemPolicy();
            // Build a key that we can use to cache the contents in memory against
            string cacheKey = param;

            if (param == "CONFIG")
                filepath = Path.Combine(Environment.CurrentDirectory.ToString() +"/"+ $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
            //filepath = Path.Combine(Environment.CurrentDirectory.ToString() + "/JsonData/kbzsms_config.json");
            else
                filepath = Path.Combine(Environment.CurrentDirectory.ToString() + "/JsonData/kbzsms_subscriber.json");
            // Check if the data exists in cache
            string fileContents = _cache.Get(cacheKey) as string;

            // If it is null, then go and fetch it
            if (string.IsNullOrEmpty(fileContents))
            {
                using (StreamReader streamReader = new StreamReader(filepath))
                {
                    fileContents = streamReader.ReadToEnd();
                }

                // Add back into cache with the dependency
                _policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { filepath }));
                _cache.Add(cacheKey, fileContents, _policy);
            }

            return fileContents;
        }
    }
}