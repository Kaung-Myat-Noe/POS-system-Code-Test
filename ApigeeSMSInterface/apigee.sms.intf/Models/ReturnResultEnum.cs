using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace apigee.sms.intf.Models
{
    public class ReturnResultEnum
    {
        public enum returnResultEnum
        {
            [Description("Process Pending")]
            Pending = 0,
            [Description("Process Success")]
            Success = 1,
            [Description("Process Fail")]
            Fail = 2,
            [Description("Parameter Empty. Please Add patameter")]
            Empty_Paramater = 3,
            [Description("SMS Sent!")]
            Success_SMS_Sent = 4,
            [Description("Couldn't Send SMS!")]
            Fail_SMS_Sent = 5,
            [Description("Cache Updated Successfully")]
            Success_Cache_Update = 6,
            [Description("Failed to Update Cache")]
            Fail_Cache_Update = 7,
            [Description("VALUE or KEY cannot be Null")]
            Fail_VALUE_KEY_NULL = 8,
            [Description("KEY cannot be Null")]
            Fail_KEY_NULL = 9,
            [Description("No Record Found")]
            Fail_No_Record = 10,
            [Description("Invalid Subscriber Number")]
            Fail_SMS_Invalid_SubscriberNum = 11,
            [Description("Please Update Phone Prefix Cache")]
            Update_Prefix_Cache= 12,
            [Description("Cache Couldn't Be Found. Please Update Cache From Portal!")]
            Update_Cache = 12,

        }
        public enum FilterEnum
        {
            [Description("By Client")]
            By_Client = 0,
            [Description("By Client and Telco")]
            By_Client_Telco = 1,
            [Description("By Telco")]
            By_Telco = 2,

        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString()) ?? throw new ArgumentNullException();

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[] ?? throw new ArgumentNullException(); ;

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }


}