﻿using AwsSqsServiceAstha.Entities;
using AwsSqsServiceAstha.Providers;
using AwsSqsServiceAstha.QResponse;
using FIK.DAL;
using System;
using System.Collections.Generic;

namespace AwsSqsServiceAstha
{
    public class AwsSqsRepository : BaseRepository
    {
        public long orderId;
        public AwsSqsRepository()
        {

        }
        #region OrderManager
        public bool OrderManager(OrderResponse response, out string errMsg)
        {
            errMsg = string.Empty;
            CompositeModel composite = new CompositeModel();
            if(response.data.status == "0")
            {
                response.data.status = "A";
            }
            if ((response.data.status == "A" && response.actionDetails != null && response.actionDetails.type == "ORDER_ALLOCATION" && string.IsNullOrEmpty(response.data.originalOrderId)) ||
                (response.data.status == "A" && response.actionDetails != null && response.actionDetails.type == "ORDER_ALLOCATION" && response.data.subStatus == "RL" && string.IsNullOrEmpty(response.data.originalOrderId)) ||
                (response.data.status == "A" && response.data.subStatus == "RS"))//|| response.data.status == "D" || response.data.status == "C"
            {
                string updateCol = @"OrderLineId,ResponseDate,ParentOrderlineId,TotalVoucherDiscount,OrderId,Description,DerivedStatusCode,DerivedStatus,TotalPromotionDiscount,DeliveryMode,ItemStatus,VariantProductId,ProductId,IsPrimaryProduct,SKU,VariantSku,Quantity,ShippingCost,ShippingVoucherDiscount,ProductTitle,CancelQuantity,IsParentProduct,IsBackOrder,TotalTaxAmount,locationCode,BundleProductId,ProductPrice,StockAction,ReturnStatus,TransferStatus";

                List<OrderLine> dbOrderLst = _dal.Select<OrderLine>("select * from EC_OrderLine where OrderId='" + response.data.orderId + "'", ref msg);
                if (dbOrderLst != null && dbOrderLst.Count > 0)
                {
                    foreach (var dbOrder in dbOrderLst)
                    {
                        var data = response.data.orderLineId.Find(a => a.VariantSku == dbOrder.VariantSku && a.orderId == Convert.ToString(dbOrder.OrderId));

                        if (Convert.ToString(dbOrder.OrderId) == data.orderId && dbOrder.VariantSku == data.VariantSku && dbOrder.locationCode == data.locationCode)
                        {
                            if (dbOrder.TransferStatus == "Y")
                            {
                                dbOrder.TransferStatus = "Y";
                                dbOrder.ResponseDate = DateTime.Now;
                                composite.AddRecordSet<OrderLine>(dbOrder, OperationMode.Update, "", updateCol, "OrderId,VariantSku", "EC_OrderLine");

                            }
                            else if (dbOrder.TransferStatus == "N")
                            {
                                dbOrder.TransferStatus = "N";
                                dbOrder.ResponseDate = DateTime.Now;
                                composite.AddRecordSet<OrderLine>(dbOrder, OperationMode.Update, "", updateCol, "OrderId,VariantSku", "EC_OrderLine");
                            }
                        }
                        else if (Convert.ToString(dbOrder.OrderId) == data.orderId && dbOrder.VariantSku == data.VariantSku && dbOrder.locationCode != data.locationCode)
                        {
                            Order model = PrepareData(response);
                            model.TransferStatus = "N";

                            composite.AddRecordSet<OrderLine>(model.orderLine.Find(m => m.VariantSku == dbOrder.VariantSku), OperationMode.Update, "", updateCol, "OrderId,VariantSku", "EC_OrderLine");
                            if (dbOrder.TransferStatus == "N")
                            {
                                dbOrder.TransferStatus = "Skip";
                                dbOrder.ResponseDate = DateTime.Now;
                                composite.AddRecordSet<OrderLine>(dbOrder, OperationMode.Insert, "AutoOrderId", "", "OrderId,VariantSku", "EC_OrderLineHistory");
                            }
                            if (dbOrder.TransferStatus == "Y")
                            {
                                dbOrder.ResponseDate = DateTime.Now;
                                dbOrder.TransferStatus = "N";
                                composite.AddRecordSet<OrderLine>(dbOrder, OperationMode.Insert, "", "", "OrderId,VariantSku", "EC_OrderLineHistory");
                            }
                        }
                    }
                    var lineResponse = _dal.InsertUpdateComposite(composite, ref msg);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        errMsg = msg;
                    }
                    return lineResponse;
                }
                else
                {
                    try
                    {
                        Order model = PrepareData(response);

                        composite.AddRecordSet<Order>(model, OperationMode.InsertOrUpdaet, "", "", "OrderId", "EC_Order");
                        composite.AddRecordSet<OrderLine>(model.orderLine, OperationMode.InsertOrUpdaet, "", "", "OrderId,VariantSku", "EC_OrderLine");
                        composite.AddRecordSet<PaymentDetails>(model.paymentDetails, OperationMode.InsertOrUpdaet, "", "", "OrderId", "EC_PaymentDetails");
                        var res = _dal.InsertUpdateComposite(composite, ref msg);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            errMsg = msg;
                        }
                        return res;
                    }
                    catch (Exception e)
                    {

                        errMsg = e.Message;
                        return false;
                    }
                }

            }
            // orginal order id logic
            else if (response.data.status == "A" && response.actionDetails != null && response.actionDetails.type == "ORDER_ALLOCATION" && !string.IsNullOrEmpty(response.data.originalOrderId))
            {
                //string updateCol = @"OrderLineId,ResponseDate,ParentOrderlineId,TotalVoucherDiscount,OrderId,Description,DerivedStatusCode,DerivedStatus,TotalPromotionDiscount,DeliveryMode,ItemStatus,VariantProductId,ProductId,IsPrimaryProduct,SKU,VariantSku,Quantity,ShippingCost,ShippingVoucherDiscount,ProductTitle,CancelQuantity,IsParentProduct,IsBackOrder,TotalTaxAmount,locationCode,BundleProductId,ProductPrice,StockAction,ReturnStatus,TransferStatus";
                var model = PrepareData(response);

                #region Original order
                //List<OrderLine> dbOrgOrderLst = _dal.Select<OrderLine>("select * from EC_OrderLineShipment where OrderId='" + response.data.originalOrderId + "'", ref msg);
                //if (dbOrgOrderLst != null && dbOrgOrderLst.Count > 0)
                //{
                //    List<string> sqlList = new List<string>();
                //    foreach (var dbOrder in dbOrgOrderLst)
                //    {
                //        dbOrder.CancelQty = dbOrder.Quantity;
                //        dbOrder.TransferStatus = "N";
                //        sqlList.Add("update EC_OrderLineShipment set TransferStatus='N',CancelQty='" + dbOrder.Quantity + "' where  OrderID='" + response.data.originalOrderId + "' AND  VariantSku='" + dbOrder.VariantSku + "'");
                //    }

                //   _dal.ExecuteQuery(sqlList, ref msg);
                //}
                //else
                //{
                //    errMsg = $"Orginal order ID {response.data.originalOrderId} Not found";
                //    return false;
                //}
                #endregion

                #region new order
                List<OrderLine> dbOrderLst = _dal.Select<OrderLine>("select * from EC_OrderLine where OrderId='" + response.data.orderId + "'", ref msg);

                if (dbOrderLst == null || dbOrderLst.Count == 0)
                {
                    Order modelNew = PrepareData(response);
                    composite.AddRecordSet<Order>(modelNew, OperationMode.InsertOrUpdaet, "", "", "OrderId", "EC_Order");
                    composite.AddRecordSet<OrderLine>(modelNew.orderLine, OperationMode.InsertOrUpdaet, "", "", "OrderId,VariantSku", "EC_OrderLine");
                    composite.AddRecordSet<PaymentDetails>(modelNew.paymentDetails, OperationMode.InsertOrUpdaet, "", "", "OrderId", "EC_PaymentDetails");
                }
                #endregion


                try
                {
                    var res = _dal.InsertUpdateComposite(composite, ref msg);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        errMsg = msg;
                    }
                    return res;
                }
                catch (Exception e)
                {
                    errMsg = e.Message;
                    return false;
                }
            }
            //order cancel block
            else if (response.changedAttributes != null)
            {

                try
                {
                    Order model = PrepareData(response);

                    var ObjOrderLine = response.data.orderLineId.Find(a => a.derivedStatusCode == "IC" || a.derivedStatusCode == "ICI");

                    if (ObjOrderLine != null)
                    {
                        var OrderObj = _dal.SelectFirstOrDefault<Order>("select * from EC_Order where OrderId='" + response.data.orderId + "'", ref msg);
                        if (OrderObj == null)
                        {
                            errMsg = "Order cancel not possible beacuse this order data not exist in our system";
                            return false;
                        }
                        string updateCol = @"OrderLineId,ResponseDate,ParentOrderlineId,TotalVoucherDiscount,OrderId,Description,DerivedStatusCode,DerivedStatus,TotalPromotionDiscount,DeliveryMode,ItemStatus,VariantProductId,ProductId,IsPrimaryProduct,SKU,VariantSku,Quantity,ShippingCost,ShippingVoucherDiscount,ProductTitle,CancelQuantity,IsParentProduct,IsBackOrder,TotalTaxAmount,locationCode,BundleProductId,ProductPrice,StockAction,ReturnStatus,TransferStatus";
                        composite.AddRecordSet<OrderLine>(model.orderLine.FindAll((a => a.DerivedStatusCode == "IC" || a.DerivedStatusCode == "ICI")), OperationMode.InsertOrUpdaet, "", updateCol, "OrderId,VariantSku", "EC_OrderLineCancel");

                        var res = _dal.InsertUpdateComposite(composite, ref msg);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            errMsg = msg;
                        }
                        return res;
                    }
                    else
                    {
                        errMsg = "No data found on database . Order is not allocated";
                        return false;
                    }

                }
                catch (Exception e)
                {

                    errMsg = e.Message;
                    return false;
                }
            }



            else
            {
                errMsg = "Skipped.Order Status Not In Requirement";
                return false;
            }

        }

        private Order PrepareData(OrderResponse response)
        {
            Order model = new Order();
            model.AmountPayable = Convert.ToDecimal(response.data.amountPayable);
            model.ConversionFactor = Convert.ToInt32(response.data.conversionFactor);
            model.DeliveryOption = response.data.deliveryOption;
            model.IsGift = response.data.isGift;
            model.MerchantId = response.data.merchantId;
            model.OrderDate = response.data.orderDate;
            model.OrderId = Convert.ToInt64(response.data.orderId);
            orderId = model.OrderId;
            model.OriginalOrderId = string.IsNullOrEmpty(response.data.originalOrderId) ? 0 : Convert.ToInt64(response.data.originalOrderId);
            model.PickupMobile = response.data.pickupMobile;
            model.PromotionDiscount = Convert.ToDecimal(response.data.promotionDiscount);
            model.ReferenceNo = response.data.referenceNo;
            model.RefundAmount = Convert.ToDecimal(response.data.refundAmount);
            model.ReturnOrderId = string.IsNullOrEmpty(response.data.returnOrderId) ? 0 : Convert.ToInt64(response.data.returnOrderId);
            model.Rewards = Convert.ToString(response.data.rewards);
            model.ShippingDiscount = Convert.ToDecimal(response.data.shippingDiscount);
            model.ShippingMode = response.data.shippingMode;
            model.Status = response.data.status;
            model.TaxTotal = response.data.taxTotal;
            model.TotalAmount = Convert.ToDecimal(response.data.totalAmount);
            model.VoucherCode = response.data.voucherCode;
            model.VoucherDiscount = Convert.ToDecimal(response.data.voucherDiscount);
            model.Mobile = response.data.billingAddress.mobile;
            model.Address = response.data.billingAddress.address1 + response.data.billingAddress.address2;
            model.Name = response.data.billingAddress.firstname + response.data.billingAddress.lastname;
            model.Email = response.data.billingAddress.email;
            model.TransferStatus = "N";// orderline transfer status - New
            model.orderLine = orderLine(response.data.orderLineId);
            model.paymentDetails = paymentDetails(response.data.paymentDetails);

            return model;
        }

        public List<OrderLine> orderLine(List<OrderLineId> lstItem)
        {
            List<OrderLine> orderLine = new List<OrderLine>();
            foreach (var item in lstItem)
            {
                OrderLine order = new OrderLine();
                order.BundleProductId = Convert.ToInt64(item.BundleProductId);
                order.CancelQuantity = Convert.ToDecimal(item.cancelQuantity);
                order.DeliveryMode = item.deliveryMode;
                order.DerivedStatus = item.derivedStatus;
                order.DerivedStatusCode = item.derivedStatusCode;
                order.Description = item.description;
                order.IsBackOrder = item.isBackOrder;
                order.IsParentProduct = item.isParentProduct;
                order.IsPrimaryProduct = Convert.ToBoolean(item.isPrimaryProduct);
                order.ItemStatus = item.itemStatus;
                order.locationCode = item.locationCode;
                order.OrderId = Convert.ToInt64(item.orderId);
                order.OrderLineId = Convert.ToInt64(item.orderLineId);
                order.ParentOrderlineId = Convert.ToInt64(item.parentOrderlineId);
                order.ProductId = Convert.ToInt64(item.productId);
                order.ProductPrice = Convert.ToDecimal(item.productPrice);
                order.ProductTitle = item.ProductTitle;
                order.Quantity = Convert.ToDecimal(item.quantity);
                order.ShippingCost = Convert.ToDecimal(item.shippingCost);
                order.ShippingVoucherDiscount = Convert.ToDecimal(item.shippingVoucherDiscount);
                order.SKU = item.SKU;
                order.StockAction = item.stockAction;
                order.TotalPromotionDiscount = item.totalPromotionDiscount;
                order.TotalTaxAmount = item.totalTaxAmount;
                order.TotalVoucherDiscount = Convert.ToDecimal(item.totalVoucherDiscount);
                order.VariantProductId = Convert.ToInt64(item.variantProductId);
                order.VariantSku = item.VariantSku;
                order.TransferStatus = "N";
                order.ResponseDate = DateTime.Now;
                orderLine.Add(order);
            }
            return orderLine;
        }
        public List<PaymentDetails> paymentDetails(List<PaymentDetail> lstPayment)
        {
            List<PaymentDetails> paymentDetails = new List<PaymentDetails>();
            foreach (var item in lstPayment)
            {
                PaymentDetails model = new PaymentDetails();
                model.Amount = Convert.ToDecimal(item.amount);
                model.ClientIP = item.clientIP;
                model.currencyCode = item.currencyCode;
                model.OrderId = orderId;
                model.paymentDate = Convert.ToDateTime(item.paymentDate);
                model.PaymentDetailsId = Convert.ToInt64(item.paymentDetailsId);
                model.PaymentOption = item.paymentOption;
                model.PaymentStatus = item.paymentStatus;
                model.PaymentType = item.paymentType;
                model.PointsBurned = Convert.ToInt32(item.pointsBurned);
                model.ResponseCode = item.responseCode;

                paymentDetails.Add(model);
            }

            return paymentDetails;
        }
        #endregion

        #region ProductManager
        public bool ProductManager(ProductResponse response, out string errMsg)
        {
            try
            {
                errMsg = string.Empty;
                List<string> sqlList = new List<string>();
                if (!string.IsNullOrEmpty(response.newData.variantSKU))
                {
                    sqlList.Add("update  Article set  IsEc='1' where ArtNo='" + response.newData.variantSKU + "' ");
                }
                else
                {
                    sqlList.Add("update  Article set  IsEc='1' where left(ArtNo,8)='" + response.newData.productSKU + "' ");

                }
                bool resp = _dal.ExecuteQuery(sqlList, ref msg);
                if (resp == false || !string.IsNullOrEmpty(msg))
                {
                    errMsg = msg;
                    return false;
                }
                return resp;
            }
            catch (Exception ex)
            {

                errMsg = ex.Message;
                return false;
            }
        }
        #endregion

        #region ReturnManager
        public bool ReturnManager(ReturnResponse response, out string errMsg)
        {
            var orderId = response.returnRequest.orderId;
            errMsg = string.Empty;
            List<string> sqlList = new List<string>();
            try
            {
                if (response.returnRequest.requestStatus == "R")
                {
                    foreach (var item in response.returnRequest.returnRequestDetails)
                    {
                        sqlList.Add("Update EC_OrderLineShipment set TransferStatus='N', CancelQty='" + item.returnQty + "'  where OrderID='" + orderId + "' and VariantSKU='" + item.variantSKU + "'");
                    }
                    bool resp = _dal.ExecuteQuery(sqlList, ref msg);
                    if (resp == false || !string.IsNullOrEmpty(msg))
                    {
                        errMsg = msg;
                        return false;
                    }
                    return resp;
                }
                else
                {
                    errMsg = "Return not allocated.because request status:" + response.returnRequest.requestStatus;
                    return false;
                }

            }
            catch (Exception ex)
            {

                errMsg = ex.Message;
                return false;
            }
        }
        #endregion


        #region ShipmentManager
        public bool ShipmentManager(ShipmentResponse response, out string errMsg)
        {
            try
            {
                List<ShippingOrder> lstship = new List<ShippingOrder>();
                CompositeModel composite = new CompositeModel();
                errMsg = string.Empty;
                if (response.Data.ShippingStatus == "D" )
                {
                    if(response.Data.ShipmentType== "Reverse")
                    {
                        errMsg = "Skipped for reversed shipment. Shipment ID:"+response.Data.ShipmentId;
                        return false;
                    }
                    foreach (var item in response.Data.ShipmentItems)
                    {
                        ShippingOrder ship = new ShippingOrder();
                        ship.BundleProductId = item.BundleProductId;
                        ship.CategoryId = item.CategoryId;
                        ship.CategoryName = item.CategoryName;
                        ship.OrderID = response.Data.OrderId;
                        ship.OrderLineId = item.OrderLineId;
                        ship.ParentOrderLineId = item.ParentOrderLineId;
                        ship.ProductId = item.ProductId;
                        ship.Quantity = Convert.ToDecimal(item.Quantity);
                        ship.ShipmentDetailsId = item.ShipmentDetailsId;
                        ship.ShippingStatus = response.Data.ShippingStatus;                        
                        ship.SKU = item.SKU;
                        ship.Title = item.Title;
                        ship.VariantProductId = item.VariantProductId;
                        ship.VariantSKU = item.VariantSKU;
                        ship.TransferStatus = "N";
                        ship.CancelQty = 0;
                        ship.locationCode = response.Data.LocationCode;
                        ship.DocketNumber = response.Data.DocketNumber;
                        ship.ResponseDate = DateTime.Now;
                        lstship.Add(ship);
                    }
                    composite.AddRecordSet<ShippingOrder>(lstship, OperationMode.Insert, "", "", "", "EC_OrderLineShipment");
                }

                if (response.Data.ShippingStatus == "O")
                {
                    List<OrderLine> lstOrderHistory = new List<OrderLine>();
                    foreach (var item in response.Data.ShipmentItems)
                    {
                        OrderLine order = new OrderLine();
                        order.BundleProductId = 0;
                        order.DeliveryMode = "";
                        order.DerivedStatus = "RTO";
                        order.DerivedStatusCode = "O";
                        order.Description = item.Title;
                        order.IsBackOrder = true;
                        order.IsParentProduct = false;
                        order.IsPrimaryProduct = false;
                        order.ItemStatus = "";
                        order.locationCode = response.Data.LocationCode;
                        order.OrderId = Convert.ToInt64(response.Data.OrderId);
                        order.OrderLineId = Convert.ToInt64(item.OrderLineId);
                        order.ParentOrderlineId = 0;
                        order.ProductId = Convert.ToInt64(item.ProductId);
                        order.ProductPrice = 0;
                        order.ProductTitle = item.Title;
                        order.Quantity = Convert.ToDecimal(item.Quantity);
                        order.ShippingCost = 0;
                        order.ShippingVoucherDiscount = 0;
                        order.SKU = item.SKU;
                        order.StockAction = "";
                        order.TotalPromotionDiscount = 0;
                        order.TotalTaxAmount = 0;
                        order.TotalVoucherDiscount = 0;
                        order.VariantProductId = Convert.ToInt64(item.VariantProductId);
                        order.VariantSku = item.VariantSKU;
                        order.TransferStatus = "N";
                        lstOrderHistory.Add(order);
                    }
                    composite.AddRecordSet<OrderLine>(lstOrderHistory, OperationMode.Insert, "", "", "", "EC_OrderLineHistory");
                }

                var res = _dal.InsertUpdateComposite(composite, ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    errMsg = msg;
                    return false;
                }
                return res;
            }
            catch (Exception ex)
            {

                errMsg = ex.Message;
                return false;
            }
        }
        #endregion

        #region LogManager
        public void LogManager(string JsonString, string errMsg, bool response, string FromWhere, string ID)
        {
            try
            {
                //EcDataTransferLog
                TransferLog tl = new TransferLog();
                tl.Date = DateTime.Now;
                tl.Type = "AzureServiceBus";
                tl.Taskid = FromWhere;
                if (!response)
                {
                    tl.Message = JsonString;
                    tl.Status = "FAILED";
                    tl.MessageCode = ID;
                    tl.ErrorCode = ID;
                    tl.Reason = errMsg;
                }
                if (response)
                {
                    tl.Message = "ID :- " + FromWhere + " has been saved successfully." + JsonString;
                    tl.Status = "SUCCESSED";
                    tl.MessageCode = ID;
                    tl.ErrorCode = ID;
                    tl.Reason = "";
                }
                _dal.Insert<TransferLog>(tl, "", "", "EcDataTransferLog", ref msg);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region DBCheck
        public bool ConnCheck()
        {

            try
            {
                var response = _dal.Select<Order>("select top(1) OrderId from EC_Order", ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                throw e;
                return false;
            }
            // var response = _dal.Select<Order>("select top(1) OrderId from EC_Order", ref msg).Count;
            // return response > 0 ? true : false;
        }
        #endregion
    }
}
