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
                                    //var jsonString = "{\r\n   \"actionDetails\":{\r\n      \"type\":\"ORDER_ALLOCATION\"\r\n   },\r\n   \"changedAttributes\":{\r\n      \r\n   },\r\n   \"data\":{\r\n      \"orderId\":\"23071200007\",\r\n      \"originalOrderId\":\"\",\r\n      \"orderDate\":\"2023-07-12T06:02:22.4898963Z\",\r\n      \"status\":\"A\",\r\n      \"taxTotal\":0,\r\n      \"totalAmount\":8960,\r\n      \"amountPayable\":8460,\r\n      \"conversionFactor\":\"1\",\r\n      \"voucherCode\":\"\",\r\n      \"refundAmount\":\"\",\r\n      \"referenceNo\":\"\",\r\n      \"voucherDiscount\":\"\",\r\n      \"rewards\":null,\r\n      \"paymentDetails\":[\r\n         {\r\n            \"amount\":8460,\r\n            \"responseCode\":null,\r\n            \"pointsBurned\":null,\r\n            \"paymentOption\":\"COD\",\r\n            \"paymentDetailsId\":\"219754047\",\r\n            \"paymentDate\":\"7/12/2023 6:02:25 AM\",\r\n            \"currencyCode\":\"BDT\"\r\n         }\r\n      ],\r\n      \"billingAddress\":{\r\n         \"firstname\":\"Test\",\r\n         \"lastname\":\"Customer\",\r\n         \"address1\":\"Test Street 1\",\r\n         \"address2\":\"\",\r\n         \"mobile\":\"\\u002B8801926142863\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"tester@asthait.com\"\r\n      },\r\n      \"shippingAddress\":{\r\n         \"firstname\":\"Test\",\r\n         \"lastname\":\"Customer\",\r\n         \"address1\":\"Test Street 1\",\r\n         \"address2\":\"\",\r\n         \"mobile\":\"\\u002B8801926142863\",\r\n         \"countryCode\":\"BD\",\r\n         \"email\":\"tester@asthait.com\"\r\n      },\r\n      \"orderLineId\":[\r\n         {\r\n            \"orderId\":\"23071200007\",\r\n            \"orderLineId\":\"230712000071\",\r\n            \"productId\":\"647716da2740232599a6eb19\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\" VENTURINI MEN\\u0027S TOE CAP SHOE\",\r\n            \"description\":\"\\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n\\r\\n    \\r\\n    \\r\\n\\r\\n    \\u003Cdiv\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EOuter Panel: 92% Polyester, 8% Elastane\\u003C/p\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EInner Panel: 80% Polyester, 20% Elastane\\u003C/p\\u003E\\r\\n\\u003C/div\\u003E\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\",\r\n            \"SKU\":\"91118A67\",\r\n            \"VariantSku\":\"91118A6720\",\r\n            \"productPrice\":4480,\r\n            \"locationCode\":\"G184\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":250,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":\"A\",\r\n            \"derivedStatus\":\"Authorized\",\r\n            \"deliveryMode\":\"H\"\r\n         },\r\n         {\r\n            \"orderId\":\"23071200007\",\r\n            \"orderLineId\":\"230712000072\",\r\n            \"productId\":\"647716da2740232599a6eb19\",\r\n            \"BundleProductId\":\"\",\r\n            \"ProductTitle\":\" VENTURINI MEN\\u0027S TOE CAP SHOE\",\r\n            \"description\":\"\\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n    \\r\\n\\r\\n    \\r\\n    \\r\\n\\r\\n    \\u003Cdiv\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EOuter Panel: 92% Polyester, 8% Elastane\\u003C/p\\u003E\\r\\n  \\u003Cp style=\\u0022font-size:16px;color:#7C7C7C;text-align:justify\\u0022\\u003EInner Panel: 80% Polyester, 20% Elastane\\u003C/p\\u003E\\r\\n\\u003C/div\\u003E\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\",\r\n            \"SKU\":\"91118A67\",\r\n            \"VariantSku\":\"91118A6740\",\r\n            \"productPrice\":4480,\r\n            \"locationCode\":\"G184\",\r\n            \"quantity\":1,\r\n            \"cancelQuantity\":0,\r\n            \"totalTaxAmount\":0,\r\n            \"shippingVoucherDiscount\":0,\r\n            \"shippingCost\":0,\r\n            \"totalPromotionDiscount\":250,\r\n            \"totalVoucherDiscount\":0,\r\n            \"derivedStatusCode\":\"A\",\r\n            \"derivedStatus\":\"Authorized\",\r\n            \"deliveryMode\":\"H\"\r\n         }\r\n      ]\r\n   }\r\n}";
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
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Order", "AllocateOrder", StaticDetails.Order);
                                }
                            }
                            //ReturnManagement
                            else if (item.eventType.ToLower() == "returnorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String
                                    var jsonString = "{\r\n   \"returnRequest\":\"R\",\r\n   \"returnRequestDetails\":[\r\n      {\r\n         \"CancelQty\":\"1\",\r\n         \"SKU\":\"07110A4044 \",\r\n         \"VariantSKU\":\"07110A4044\",\r\n         \"OrderID\":\"23071300003\",\r\n         \"OrderLineId\":\"230713000031\",\r\n         \"locationCode\":\"64941bd8ef4d1be2891f3ccc\"\r\n      },\r\n      {\r\n         \"CancelQty\":\"1\",\r\n         \"SKU\":\"07111A2340\",\r\n         \"VariantSKU\":\"07111A2340\",\r\n         \"OrderID\":\"23071300003\",\r\n         \"OrderLineId\":\"230713000032\",\r\n         \"locationCode\":\"64941bd8ef4d1be2891f3ccc\"\r\n      }\r\n   ]\r\n}";
                                    //var jsonString = item.data.ToString();
                                    var json = JsonConvert.DeserializeObject<ReturnResponse>(jsonString);
                                    var response = repo.ReturnManager(json, out string errMsg);
                                    repo.LogManager(jsonString, errMsg, response, message.MessageId, json.returnRequestDetails.FirstOrDefault().OrderID, StaticDetails.ReturnOrder);
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                }
                                catch (Exception ex)
                                {
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Return Order", StaticDetails.ReturnOrder);
                                }
                            }
                            //ShipmentManagement
                            else if (item.eventType.ToLower() == "shipmentorder")
                            {
                                try
                                {
                                    // Serialize our concrete class into a JSON String   
                                    var jsonString = item.data.ToString();
                                    //var jsonString = "{\r\n   \"ShippingStatus\":\"O\",\r\n   \"ShipmentItems\":[\r\n      {\r\n         \"OrderID\":\"23070500011\",\r\n         \"OrderLineId\":\"230705000111\",\r\n         \"Quantity\":\"1\",\r\n         \"ShipmentDetailsId\":\"5851451851\",\r\n         \"ShippingStatus\":\"O\",\r\n         \"SKU\":\"91118A67\",\r\n         \"VariantSKU\":\"91118A6720\",\r\n         \"Title\":\"Sprint Mens Shoe\",\r\n         \"locationCode\":\"G184\"\r\n      }\r\n   ]\r\n}";
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
                                    await DeleteMessage(sqsClient, message, queueUrl);
                                    var jsonString = JsonConvert.SerializeObject(item.data);
                                    repo.LogManager(jsonString, ex.Message + ex.StackTrace, false, "Exception from Return", "Shipment Order", StaticDetails.ShipmentOrder);
                                }
                            }
                        }
                    }
                } while (isCloseForm == false);
            }
            catch (Exception ex)
            {

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
