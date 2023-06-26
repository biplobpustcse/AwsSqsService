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
                                    string variantSkuAll = string.Join(",", json.products.Select(x=> string.IsNullOrEmpty(x.VariantSku)? x.Sku: x.VariantSku));
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, "VariantSku:" + variantSkuAll);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Product", "Receive");
                                }
                            }
                            //OrderManagement
                            else if (item.eventType.ToLower() == "allocateorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    //var jsonString = "{\r\n   \"actionDetails\":{\r\n      \"type\":\"ORDER_ALLOCATION\"\r\n   },\r\n   \"changedAttributes\":{\r\n      \r\n   },\r\n   \"data\":{\r\n      \"orderId\":\"64811111111\",\r\n      \"originalOrderId\":\"\",\r\n      \"orderDate\":\"2023-06-19T06:28:59.5453583Z\",\r\n      \"status\":0,\r\n      \"promotionDiscount\":\"0\",\r\n      \"shippingDiscount\":\"\",\r\n      \"taxTotal\":0,\r\n      \"totalAmount\":0,\r\n      \"amountPayable\":1800,\r\n      \"conversionFactor\":\"1\",\r\n      \"voucherCode\":\"\",\r\n      \"refundAmount\":\"\",\r\n      \"referenceNo\":\"\",\r\n      \"voucherDiscount\":\"\",\r\n      \"rewards\":\"\",\r\n      \"paymentDetails\":[\r\n         \r\n      ],\r\n      \"shippingMode\":5,\r\n      \"deliveryOption\":0,\r\n      \"billingAddress\":\"\",\r\n      \"shippingAddress\":\"\",\r\n      \"orderLineId\":[\r\n         {\r\n            \"orderId\":\"64811111111\",\r\n            \"orderLineId\":\"648111111112\",\r\n            \"productId\":\"648111111115\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\"\",\r\n            \"description\":\"\",\r\n            \"SKU\":\"\",\r\n            \"VariantSku\":\"\",\r\n            \"productPrice\":900,\r\n            \"locationCode\":\"\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":2,\r\n            \"derivedStatus\":15,\r\n            \"deliveryMode\":0\r\n         },\r\n         {\r\n            \"orderId\":\"64811111111\",\r\n            \"orderLineId\":\"648111111113\",\r\n            \"productId\":\"648111111116\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\"\",\r\n            \"description\":\"\",\r\n            \"SKU\":\"\",\r\n            \"VariantSku\":\"\",\r\n            \"productPrice\":900,\r\n            \"locationCode\":\"\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":2,\r\n            \"derivedStatus\":15,\r\n            \"deliveryMode\":0\r\n         }\r\n      ]\r\n   }\r\n}";
                                    //var jsonString = "{\r\n   \"actionDetails\":{\r\n      \"type\":\"ORDER_ALLOCATION\"\r\n   },\r\n   \"changedAttributes\":{\r\n      \r\n   },\r\n   \"data\":{\r\n      \"orderId\":\"23062200023\",\r\n      \"originalOrderId\":\"\",\r\n      \"orderDate\":\"2023-06-22T09:21:56.0162346Z\",\r\n      \"status\":\"A\",\r\n      \"taxTotal\":0,\r\n      \"totalAmount\":8480,\r\n      \"amountPayable\":8480,\r\n      \"conversionFactor\":\"1\",\r\n      \"voucherCode\":\"\",\r\n      \"refundAmount\":\"\",\r\n      \"referenceNo\":\"\",\r\n      \"voucherDiscount\":\"\",\r\n      \"rewards\":null,\r\n      \"paymentDetails\":[\r\n         {\r\n            \"amount\":8480,\r\n            \"responseCode\":null,\r\n            \"pointsBurned\":null,\r\n            \"paymentOption\":\"COD\",\r\n            \"paymentDetailsId\":\"7DW3V9C4J5\",\r\n            \"paymentDate\":\"6/22/2023 9:21:56 AM\",\r\n            \"currencyCode\":\"BDT\"\r\n         }\r\n      ],\r\n      \"billingAddress\":{\r\n         \"firstname\":\"Fariha\",\r\n         \"lastname\":\"Jabin\",\r\n         \"address1\":\"21/3 \",\r\n         \"address2\":\"\",\r\n         \"mobile\":\"01717476191\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"fariha.jabin@asthait.com\"\r\n      },\r\n      \"shippingAddress\":{\r\n         \"firstname\":\"Fariha\",\r\n         \"lastname\":\"Jabin\",\r\n         \"address1\":\"21/3 \",\r\n         \"address2\":\"\",\r\n         \"mobile\":\"01717476191\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"fariha.jabin@asthait.com\"\r\n      },\r\n      \"orderLineId\":[\r\n         {\r\n            \"orderId\":\"23062200023\",\r\n            \"orderLineId\":\"230622000231\",\r\n            \"productId\":\"647716da2740232599a6eb19\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\" VENTURINI MEN\\u0027S TOE CAP SHOE\",\r\n            \"description\":\"\\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n\\r\\n    \\r\\n    \\r\\n\\r\\n    \\u003Cdiv\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EOuter Panel: 92% Polyester, 8% Elastane\\u003C/p\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EInner Panel: 80% Polyester, 20% Elastane\\u003C/p\\u003E\\r\\n\\u003C/div\\u003E\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\",\r\n            \"SKU\":\"SKU - 91125A81\",\r\n            \"VariantSku\":\"SKU - 91125A8139\",\r\n            \"productPrice\":4480,\r\n            \"locationCode\":\"G107\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":0,\r\n            \"derivedStatus\":0,\r\n            \"deliveryMode\":0\r\n         },\r\n         {\r\n            \"orderId\":\"23062200023\",\r\n            \"orderLineId\":\"230622000232\",\r\n            \"productId\":\"64869db91b25cdfec0504127\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\" VENTURINI MEN\\u0027S WALLET RED\",\r\n            \"description\":\"\\r\\n    \\r\\nShort description\",\r\n            \"SKU\":\"SKU - 07117A77\",\r\n            \"VariantSku\":\"SKU - 07117A77\",\r\n            \"productPrice\":3500,\r\n            \"locationCode\":\"G074\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":0,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":0,\r\n            \"derivedStatus\":0,\r\n            \"deliveryMode\":0\r\n         }\r\n      ]\r\n   }\r\n}";
                                    var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<OrderResponse>(jsonString);
                                    var response = repo.OrderManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, "OrderID:" + json.data.orderId);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Order", "Receive");
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
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, "ReturnOrder ID:" + json.returnRequest.orderId);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive");
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
                                        repo.LogManager(jsonString, errMsg, response, message.MessageId, "Shipment ID:" + json.Data.OrderId);
                                    }                                   
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Receive");
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
