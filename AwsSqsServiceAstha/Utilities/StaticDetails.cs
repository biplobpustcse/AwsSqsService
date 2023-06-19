using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AwsSqsServiceAstha.Utilities
{
    public static class StaticDetails
    {       
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //AWS
        public static string MaxMessages = ConfigurationManager.AppSettings["MaxMessages"];
        public static string WaitTime = ConfigurationManager.AppSettings["WaitTime"];

        public static string accessKey = ConfigurationManager.AppSettings["accessKey"];
        public static string secretKey = ConfigurationManager.AppSettings["secretKey"];

        public static string serviceURL = ConfigurationManager.AppSettings["serviceURL"];
        public static string AWS_AccountNumber = ConfigurationManager.AppSettings["AWS_AccountNumber"];
        public static string queueName = ConfigurationManager.AppSettings["queueName"];


    }
}
