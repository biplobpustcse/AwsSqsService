using System;
using System.Collections.Generic;
using System.Text;

namespace AwsSqsServiceAstha.Entities
{
    public class AwsSqsResponse
    {
        public string MessageId { get; set; }
        //public string SequenceNumber { get; set; }
        //public string MD5OfMessageBody { get; set; }
        public string ErrorMessage { get; set; }

        public List<AwsSqsMessage> Messages { get; set; }
        public ResponseMetadata ResponseMetadata { get; set; }           
    }    
    public class ResponseMetadata
    {
        public string RequestId { get; set; }        
    }
}
