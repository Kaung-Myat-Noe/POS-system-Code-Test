using System.Configuration;
using AspNetCore.ServiceRegistration.Dynamic;
using Json.Library.Extensions;
using Newtonsoft.Json;
using NLog;

namespace apigee.sms.intf.Helper
{
    [ScopedService]
    public class ConfigurationOperations
    {
        public static ILogger<dynamic> logger;
        public static dynamic ReadJson(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject(json);
        }
        public static void SMSJsonUpdateDynamic(dynamic configuration, string fileName, ILogger<dynamic> nlogger)
        {
            logger = nlogger;
            try
            {
                logger.LogInformation("Json Data: " + (string)JsonConvert.SerializeObject(configuration)+ " File Path: " + fileName);
                using (StreamWriter sw = new StreamWriter(File.Open(fileName, System.IO.FileMode.Append)))
                {
                    sw.Close();
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(fileName, output);
                }
                logger.LogInformation("Json Creation Successful: "+ fileName);
            }
            catch (Exception ex)
            {
                logger.LogInformation("Json File Creation Error: " + ex.ToString());
            }
        }
    }
}
