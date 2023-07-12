using System;
using System.Collections.Generic;
using System.Text;

namespace AwsSqsServiceAstha.QResponse
{
    //public class ChangedAttributes
    //{
    //    public string receivedby { get; set; }
    //    public string shippingstatus { get; set; }
    //}

    //public class MetaData
    //{
    //    public long messageSentAt { get; set; }
    //}

    public class ShipmentItem
    {
        public string OrderID { get; set; }
        public string locationCode { get; set; }
        public string VariantSKU { get; set; }
        public string CategoryId { get; set; }
        public string VariantProductId { get; set; }
        public string ParentOrderLineId { get; set; }
        public string OrderLineId { get; set; }
        public string Quantity { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string ProductId { get; set; }
        public string ShipmentDetailsId { get; set; }
        public string SKU { get; set; }
        public string BundleProductId { get; set; }
        public string DocketNumber { get; set; }
    }

    public class DeliverySlots
    {
        public string SlotId { get; set; }
        public string EndTime { get; set; }
        public string StartTime { get; set; }
    }

    //public class ShippingAddress
    //{
    //    public string Zip { get; set; }
    //    public string Email { get; set; }
    //    public string FirstName { get; set; }
    //    public string StateCode { get; set; }
    //    public string Address2 { get; set; }
    //    public string Latitude { get; set; }
    //    public string City { get; set; }
    //    public string Address1 { get; set; }
    //    public string Mobile { get; set; }
    //    public string Longitude { get; set; }
    //    public string CityID { get; set; }
    //    public string State { get; set; }
    //    public string Phone { get; set; }
    //    public string Country { get; set; }
    //    public string LastName { get; set; }
    //    public string CountryCode { get; set; }
    //}

    public class ShipmentTrip
    {
    }

    public class ShipmentResponseData
    {
        public string ShipmentId { get; set; }
        public string DemandedDeliveryDate { get; set; }
        public string CollectableAmount { get; set; }
        public string InvoiceAmount { get; set; }
        public string MerchantId { get; set; }
        public List<ShipmentItem> ShipmentItems { get; set; }
        public string ShipmentType { get; set; }
        public List<object> CustomAttributes { get; set; }
        public string ShippingDate { get; set; }
        public DeliverySlots DeliverySlots { get; set; }
        public string LocationId { get; set; }
        public string LocationCode { get; set; }
        public string DocketNumber { get; set; }
        public string TotalAmount { get; set; }
        public string EstimatedDeliveryDate { get; set; }
        public string OrderId { get; set; }
        public string ChannelID { get; set; }
        public string OrderDate { get; set; }
        public string ReceivedBy { get; set; }
        public string DispatchDate { get; set; }
        public string OrderConfirmedDate { get; set; }
        public string ShippingCharges { get; set; }
        public string ShippingStatus { get; set; }
        public string ServiceProvider { get; set; }
        public ShipmentTrip ShipmentTrip { get; set; }
        public string DeliveryMode { get; set; }
    }

    public class ShipmentResponse
    {
        //public ChangedAttributes ChangedAttributes { get; set; }
        //public MetaData metaData { get; set; }
        //public ShipmentResponseData data { get; set; }

        public string ShippingStatus { get; set; }
        public List<ShipmentItem> ShipmentItems { get; set; }
    }
}
