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
                                    //var jsonString = "{ \"products\": [\r\n        {\r\n            \"Sku\": \"T726N\",\r\n            \"VariantSku\": \"121313\",\r\n            \"IsEc\": true\r\n        },\r\n        {\r\n            \"Sku\": \"T726N\",\r\n            \"VariantSku\": \"121313\",\r\n            \"IsEc\": false\r\n        }] \r\n     }";
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
                                    var response = repo.ProductManager(json, out string errMsg);
                                    string variantSkuAll = string.Join(",", json.products.Select(x => string.IsNullOrEmpty(x.VariantSku) ? x.Sku : x.VariantSku));
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, variantSkuAll, StaticDetails.VariantSku);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Product", "Receive", StaticDetails.VariantSku);
                                }
                            }
                            //OrderManagement
                            else if (item.eventType.ToLower() == "allocateorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    //var jsonString = "{\r\n   \"actionDetails\":{\r\n      \"type\":\"ORDER_ALLOCATION\"\r\n   },\r\n   \"changedAttributes\":{\r\n      \r\n   },\r\n   \"data\":{\r\n      \"orderId\":\"23070300002\",\r\n      \"originalOrderId\":\"\",\r\n      \"orderDate\":\"2023-07-03T08:44:37.88753Z\",\r\n      \"status\":\"A\",\r\n      \"taxTotal\":0,\r\n      \"totalAmount\":6450,\r\n      \"amountPayable\":6450,\r\n      \"conversionFactor\":\"1\",\r\n      \"voucherCode\":\"\",\r\n      \"refundAmount\":\"\",\r\n      \"referenceNo\":\"\",\r\n      \"voucherDiscount\":\"\",\r\n      \"rewards\":null,\r\n      \"paymentDetails\":[\r\n         {\r\n            \"amount\":6450,\r\n            \"responseCode\":null,\r\n            \"pointsBurned\":null,\r\n            \"paymentOption\":\"COD\",\r\n            \"paymentDetailsId\":\"1815997069\",\r\n            \"paymentDate\":\"7/3/2023 8:44:41 AM\",\r\n            \"currencyCode\":\"BDT\"\r\n         }\r\n      ],\r\n      \"billingAddress\":{\r\n         \"firstname\":\"Testzxaxas\",\r\n         \"lastname\":\"002xzsa\",\r\n         \"address1\":\"test\",\r\n         \"address2\":\"ass\",\r\n         \"mobile\":\"\\u002B8801926142863\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"tester@asthait.com\"\r\n      },\r\n      \"shippingAddress\":{\r\n         \"firstname\":\"Testzxaxas\",\r\n         \"lastname\":\"002xzsa\",\r\n         \"address1\":\"test\",\r\n         \"address2\":\"ass\",\r\n         \"mobile\":\"\\u002B8801926142863\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"tester@asthait.com\"\r\n      },\r\n      \"orderLineId\":[\r\n         {\r\n            \"orderId\":\"23070300002\",\r\n            \"orderLineId\":\"230703000021\",\r\n            \"productId\":\"6481878a170b8383814fd5fa\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\"VENTURINI MEN\\u0027S FORMAL SHOE VERSION\",\r\n            \"description\":\"\\r\\n    \\r\\n    \\r\\n    \\r\\n    \\u003Cdiv style=\\u0022font-size:16px;\\u0022\\u003EThe RS-X is back. The future-retro silhouette of this sneaker returns with a progressive aesthetic and angular details, complete with nubuck and suede overlays. The combo\\u2019s all about a disruptive design to showcase your disruptive style.\\u003C/div\\u003E\\r\\n\\r\\n\\r\\n\\r\\n\",\r\n            \"SKU\":\"07116A53 \",\r\n            \"VariantSku\":\"07116A5339\",\r\n            \"productPrice\":4490,\r\n            \"locationCode\":\"G091\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":\"A\",\r\n            \"derivedStatus\":\"Authorized\",\r\n            \"deliveryMode\":\"H\"\r\n         },\r\n         {\r\n            \"orderId\":\"23070300002\",\r\n            \"orderLineId\":\"230703000022\",\r\n            \"productId\":\"6486fadbe00345c3700d8d13\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\"TWINKLER GIRL\\u0027S PUMPY\",\r\n            \"description\":\"\\u003Cul style=\\u0022margin-right: 0px; margin-bottom: 0px; margin-left: 0px; padding: 0px; color: rgb(0, 0, 0); font-family: lato, sans-serif;\\u0022\\u003E\\u003Cli style=\\u0022margin: 0px; padding: 0px; line-height: 1.5em;\\u0022\\u003E\\u003Cul\\u003E\\u003Cli style=\\u0022margin-right: 0px; margin-bottom: 0px; margin-left: 0px; padding: 2px 0px 5px;\\u0022\\u003ELightweight and non-slip sole.\\u0026nbsp;\\u003C/li\\u003E\\u003Cli style=\\u0022margin: 0px; padding: 0px; line-height: 1.5em;\\u0022\\u003E\\u003Cp style=\\u0022margin-right: 0px; margin-bottom: 0px; margin-left: 0px; padding: 2px 0px 5px;\\u0022\\u003EComfortable PU upper\\u003C/p\\u003E\\u003C/li\\u003E\\u003C/ul\\u003E\\u003C/li\\u003E\\u003C/ul\\u003E\\r\\n    \\r\\n\",\r\n            \"SKU\":\"678677\",\r\n            \"VariantSku\":67867742,\r\n            \"productPrice\":1960,\r\n            \"locationCode\":\"G107\",\r\n            \"quantity\":4,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":\"A\",\r\n            \"derivedStatus\":\"Authorized\",\r\n            \"deliveryMode\":\"H\"\r\n         }\r\n      ]\r\n   }\r\n}";
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<OrderResponse>(jsonString);
                                    var response = repo.OrderManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, json.data.orderId, StaticDetails.Order);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Order", "Receive", StaticDetails.Order);
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
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, json.returnRequest.orderId, StaticDetails.ReturnOrder);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive", StaticDetails.ReturnOrder);
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
                                    if (json.Data != null)
                                    {
                                        var response = repo.ShipmentManager(json, out string errMsg);
                                        repo.LogManager(jsonString, errMsg, response, message.MessageId, json.Data.OrderId, StaticDetails.ShipmentOrder);
                                    }
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive", StaticDetails.ShipmentOrder);
                                }
                            }
                            else
                            {
                                var ord = "Not Match";
                            }
                        }
                    }
                } while (isCloseForm == false);
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
            await sqsClient.DeleteMessageAsync(qUrl, message.ReceiptHandle);
        }

        #endregion       

        private void Form1_Load(object sender, EventArgs e)
        {
            if (repo.ConnCheck())
            {
                _ = Task.Run(() => ReceivedataFromAwsSqs());
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
