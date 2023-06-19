using System;
using System.Collections.Generic;
using System.Text;

namespace AwsSqsServiceAstha.Entities
{
    public class AwsSqsMessage
    {        
        public string eventType { get; set; }
        public object data { get; set; }
    }    
}
