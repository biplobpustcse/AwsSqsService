using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AwsSqsServiceAstha.Entities;
using AwsSqsServiceAstha.QResponse;
using AwsSqsServiceAstha.Utilities;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Amazon.SQS.Model.Message;

namespace AwsSqsServiceAstha
{
    public partial class Form1 : Form
    {
        bool isCloseForm = false;
        //AWS Credential
        public static int maxMessages = Convert.ToInt32(StaticDetails.MaxMessages);
        public static int waitTime = Convert.ToInt32(StaticDetails.WaitTime);

        public static string accessKey = StaticDetails.accessKey;
        public static string secretKey = StaticDetails.secretKey;

        public static string serviceURL = StaticDetails.serviceURL;
        public static string AWS_AccountNumber = StaticDetails.AWS_AccountNumber;
        public static string queueName = StaticDetails.queueName;



        AwsSqsRepository repo = new AwsSqsRepository();
        public Form1()
        {
            InitializeComponent();
        }
        #region OrderManagement

        public async void ReceivedataFromAwsSqs()
        {
            AwsSqsResponse resobj = new AwsSqsResponse();
            try
            {
                var sqsConfig = new AmazonSQSConfig
                {
                    RegionEndpoint = RegionEndpoint.EUCentral1,
                    ServiceURL = serviceURL
                };
                var cred = new BasicAWSCredentials(accessKey, secretKey);
                // Create the Amazon SQS client
                var sqsClient = new AmazonSQSClient(cred, sqsConfig);
                string queueUrl = serviceURL + "/" + AWS_AccountNumber + "/" + queueName;

                do
                {
                    var receiveMessageResponse = await GetMessage(sqsClient, queueUrl, maxMessages, waitTime);
                    if (receiveMessageResponse.Messages.Count != 0)
                    {
                        foreach (var message in receiveMessageResponse.Messages)
                        {
                            var item = JsonConvert.DeserializeObject<AwsSqsMessage>(message.Body);
                            if (item.eventType.ToLower() == "allocateorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String   
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<OrderResponse>(jsonString);
                                    var response = repo.OrderManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, "OrderID:" + json.data.orderId, json.data.orderId);
                                    if (!string.IsNullOrEmpty(errMsg))
                                    {
                                        await DeleteMessage(sqsClient, message, queueUrl);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Order", "Receive");
                                }
                            }
                            else if (item.eventType.ToLower() == "returnorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String   
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ReturnResponse>(jsonString);
                                    var response = repo.ReturnManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, "ReturnOrder ID:" + json.returnRequest.orderId, json.returnRequest.orderId);
                                    if (!string.IsNullOrEmpty(errMsg))
                                    {
                                        await DeleteMessage(sqsClient, message, queueUrl);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive");
                                }
                            }                            
                            else if (item.eventType.ToLower() == "shipmentorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String   
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ShipmentResponse>(jsonString);
                                    var response = repo.ShipmentManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, "Shipment ID:" + json.Data.OrderId, json.Data.OrderId);
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive");
                                }
                            }
                        }
                    }
                } while (isCloseForm == true);
            }
            catch (Exception ex)
            {
                resobj.ErrorMessage = ex.Message;
            }
        }

        private async Task<ReceiveMessageResponse> GetMessage(IAmazonSQS sqsClient, string queueUrl, int maxNoOfMessages = 1, int waitTimeSec = 0)
        {
            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.EUCentral1,
                ServiceURL = serviceURL
            };
            // receiving the message from the queue  
            var sendMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = maxNoOfMessages,
                WaitTimeSeconds = waitTimeSec
            };
            return await sqsClient.ReceiveMessageAsync(sendMessageRequest);
        }
        // Method to delete a message from a queue
        private async Task DeleteMessage(IAmazonSQS sqsClient, Message message, string qUrl)
        {
            //Console.WriteLine($"\nDeleting message {message.MessageId} from queue...");
            await sqsClient.DeleteMessageAsync(qUrl, message.ReceiptHandle);
        }


        #endregion

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            string s = exceptionReceivedEventArgs.Exception.ToString();
            return Task.CompletedTask;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (repo.ConnCheck())
            {
                _ = Task.Run(() => ReceivedataFromAwsSqs());
                // _ = Task.Run(() => ProductManagement());//blocked only for testing because we have no test credentials
                // _ = Task.Run(() => ReturnManagement());
                // _ = Task.Run(() => ShipmentManagement());
            }
            else
            {
                MessageBox.Show("Database Connection Error");
                lblDb.Text = "Database Connection Error";
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(lblDb.Text))
            {
                isCloseForm = true;

                //orderClient.CloseAsync();
                //productClient.CloseAsync();
                //returnClient.CloseAsync();
            }

        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void AddbuttonClick(object sender, EventArgs e)
        {
            JsonForm jForm = new JsonForm();
            jForm.ShowDialog();
        }
    }
}
