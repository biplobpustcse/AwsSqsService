using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AwsSqsServiceAstha.Entities;
using AwsSqsServiceAstha.QResponse;
using AwsSqsServiceAstha.Utilities;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Linq;
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
        #region ReceivedataFromAwsSqs

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
                            //ProductManagement
                            if (item.eventType.ToLower() == "updateproduct")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
                                    var response = repo.ProductManager(json, out string errMsg);
                                    string variantSkuAll = string.Join(",", json.products.Select(x => string.IsNullOrEmpty(x.VariantSku) ? x.Sku : x.VariantSku));
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, variantSkuAll, StaticDetails.VariantSku);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Product", "Receive", StaticDetails.VariantSku);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                            }
                            //OrderManagement
                            else if (item.eventType.ToLower() == "allocateorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<OrderResponse>(jsonString);
                                    var response = repo.OrderManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, json.data.orderId, StaticDetails.Order);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Order", "AllocateOrder", StaticDetails.Order);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                            }
                            //ReturnManagement
                            else if (item.eventType.ToLower() == "returnorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ReturnResponse>(jsonString);
                                    var response = repo.ReturnManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, json.returnRequestDetails.FirstOrDefault().OrderID, StaticDetails.ReturnOrder);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Return Order", StaticDetails.ReturnOrder);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                            }
                            //ShipmentManagement
                            else if (item.eventType.ToLower() == "shipmentorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String   
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ShipmentResponse>(jsonString);
                                    if (json.ShipmentItems != null && json.ShipmentItems.Count > 0)
                                    {
                                        var response = repo.ShipmentManager(json, out string errMsg);
                                        repo.LogManager(jsonString, errMsg, response, message.MessageId, json.ShipmentItems.FirstOrDefault().OrderID, StaticDetails.ShipmentOrder);
                                    }
                                    else if (!string.IsNullOrEmpty(jsonString))
                                    {
                                        repo.LogManager(jsonString, "json format not match", false, message.MessageId, "Shipment Order", StaticDetails.ShipmentOrder);
                                    }
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Shipment Order", StaticDetails.ShipmentOrder);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                            }
                        }
                    }
                } while (isCloseForm == false);
            }
            catch (Exception ex)
            {
                try
                {
                    repo.LogManager("ReceivedataFromAwsSqs Method working failed", ex.Message + ex.StackTrace, false, "Exception From ReceivedataFromAwsSqs Method", "JSONForm", "Exception");

                    if (repo.ConnCheck())
                    {
                        ReceivedataFromAwsSqs();
                    }
                    else
                    {
                        MessageBox.Show("Database Connection Error");
                        lblDb.Text = "Database Connection Error";
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message);
                    lblDb.Text = ex2.Message;
                }
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
            await sqsClient.DeleteMessageAsync(qUrl, message.ReceiptHandle);
        }

        #endregion       

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (repo.ConnCheck())
                {
                    //_ = Task.Run(() => ReceivedataFromAwsSqs());
                    ReceivedataFromAwsSqs();
                }
                else
                {
                    MessageBox.Show("Database Connection Error");
                    lblDb.Text = "Database Connection Error";
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message);
                lblDb.Text = ex2.Message;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(lblDb.Text))
            {
                isCloseForm = true;
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
