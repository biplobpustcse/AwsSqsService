using AwsSqsServiceAstha.QResponse;
using AwsSqsServiceAstha.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AwsSqsServiceAstha
{
    public partial class JsonForm : Form
    {
        AwsSqsRepository repo = new AwsSqsRepository();
        public JsonForm()
        {
            InitializeComponent();
        }

        private void SaveButtonClick_Click(object sender, EventArgs e)
        {
            if (JsontypeDDL.SelectedIndex == 0)
            {
                MessageBox.Show("Please select JSON type to continue");
                return;
            }
            if (string.IsNullOrEmpty(JsonObjectTxtBox.Text))
            {
                MessageBox.Show("JSON Object can not be EMPTY");
                return;
            }
            if (JsontypeDDL.SelectedItem.ToString() == "ORDER")
            {
                OrderResponse json = new OrderResponse();
                try
                {
                    var jsonString = JsonObjectTxtBox.Text;
                    try
                    {
                        json = JsonConvert.DeserializeObject<OrderResponse>(jsonString);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Invalid JSON Format." + ex.StackTrace.Substring(5, 45));
                        return;
                    }
                    var response = repo.OrderManager(json, out string errMsg);
                    if (response == true && string.IsNullOrEmpty(errMsg))
                    {
                        MessageBox.Show("Operation Successfull");
                    }
                    else
                    {
                        MessageBox.Show("Response Invalid. See log for more details");
                    }

                    repo.LogManager(jsonString, errMsg, response, "OrderID:" + json.data.orderId, json.data.orderId, StaticDetails.Order);
                }
                catch (Exception ex)
                {

                    repo.LogManager(JsonObjectTxtBox.Text, ex.Message + ex.StackTrace, false, "Exception From Manual Order Insert", "JSONForm", StaticDetails.Order);
                }
            }
            if (JsontypeDDL.SelectedItem.ToString() == "ARTICLE")
            {
                ProductResponse json = new ProductResponse();
                try
                {
                    var jsonString = JsonObjectTxtBox.Text;
                    try
                    {
                        json = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Invalid JSON Format." + ex.StackTrace.Substring(5, 45));
                        return;
                    }
                    var response = repo.ProductManager(json, out string errMsg);
                    if (response == true && string.IsNullOrEmpty(errMsg))
                    {
                        MessageBox.Show("Operation Successfull");
                    }
                    else
                    {
                        MessageBox.Show("Response Invalid. See log for more details");
                    }
                    repo.LogManager(jsonString, errMsg, response, "ProductID:" + json.newData.productId, json.newData.productId, StaticDetails.VariantSku);
                }
                catch (Exception ex)
                {

                    repo.LogManager(JsonObjectTxtBox.Text, ex.Message + ex.StackTrace, false, "Exception From Manual Product Insert", "JSONForm", StaticDetails.VariantSku);
                }
            }
            if (JsontypeDDL.SelectedItem.ToString() == "RETURN")
            {
                ReturnResponse json = new ReturnResponse();
                try
                {
                    var jsonString = JsonObjectTxtBox.Text;
                    dynamic obj = JsonConvert.DeserializeObject(jsonString);
                    var ss = JsonConvert.SerializeObject(obj.data);
                    try
                    {
                        json = JsonConvert.DeserializeObject<ReturnResponse>(ss);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid JSON Format." + ex.StackTrace.Substring(5, 45));
                        return;
                    }
                    var response = repo.ReturnManager(json, out string errMsg);
                    if (response == true && string.IsNullOrEmpty(errMsg))
                    {
                        MessageBox.Show("Operation Successfull");
                    }
                    else
                    {
                        MessageBox.Show("Response Invalid. See log for more details");
                    }
                    repo.LogManager(jsonString, errMsg, response, "ReturnOrder ID:" + json.returnRequest.orderId, json.returnRequest.orderId, StaticDetails.ReturnOrder);
                }
                catch (Exception ex)
                {
                    repo.LogManager(JsonObjectTxtBox.Text, ex.Message + ex.StackTrace, false, "Exception From Manual Return Insert", "JSONForm", StaticDetails.ReturnOrder);
                }
            }
            if (JsontypeDDL.SelectedItem.ToString() == "SHIPMENT")
            {
                ShipmentResponse json = new ShipmentResponse();
                try
                {
                    var jsonString = JsonObjectTxtBox.Text;
                    try
                    {
                        json = JsonConvert.DeserializeObject<ShipmentResponse>(jsonString);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Invalid JSON Format." + ex.StackTrace.Substring(5, 45));
                        return;
                    }
                    var response = repo.ShipmentManager(json, out string errMsg);
                    if (response == true && string.IsNullOrEmpty(errMsg))
                    {
                        MessageBox.Show("Operation Successfull");
                    }
                    else
                    {
                        MessageBox.Show("Response Invalid. See log for more details");
                    }
                    repo.LogManager(jsonString, errMsg, response, "ShipmentID:" + json.Data.OrderId, json.Data.OrderId, StaticDetails.ShipmentOrder);
                }
                catch (Exception ex)
                {
                    repo.LogManager(JsonObjectTxtBox.Text, ex.Message + ex.StackTrace, false, "Exception From Manual Product Insert", "JSONForm", StaticDetails.ShipmentOrder);
                }
            }


        }

        private void CancelButtonClick_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void JsonForm_Load(object sender, EventArgs e)
        {
            JsontypeDDL.SelectedIndex = 0;
        }
    }
}
