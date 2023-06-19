using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AwsSqsServiceAstha.Utilities
{
    public static class StaticDetails
    {
        public static string Endpoint = ConfigurationManager.AppSettings["Endpoint"];
        public static string Topic = ConfigurationManager.AppSettings["Topic"];
        public static string OrderSubscription = ConfigurationManager.AppSettings["OrderSubscription"];
        public static string ProductSubscription = ConfigurationManager.AppSettings["ProductSubscription"];
        public static string LocationSubscription = ConfigurationManager.AppSettings["LocationSubscription"];
        public static string ReturnSubscription = ConfigurationManager.AppSettings["ReturnSubscription"];
        public static string ShipmentSubscription = ConfigurationManager.AppSettings["ShipmentSubscription"];
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
