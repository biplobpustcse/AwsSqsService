using System;
using System.Collections.Generic;
using System.Text;

namespace AwsSqsServiceAstha.Entities
{
	public class ShippingOrder
	{
		public string VariantSKU { get; set; }
		public string CategoryId { get; set; }
		public string VariantProductId { get; set; }
		public string ParentOrderLineId { get; set; }
		public string OrderLineId { get; set; }
		public decimal Quantity { get; set; }
		public string Title { get; set; }
		public string CategoryName { get; set; }
		public string ProductId { get; set; }
		public string ShipmentDetailsId { get; set; }
		public string SKU { get; set; }
		public string BundleProductId { get; set; }
		public string OrderID { get; set; }
		public string ShippingStatus { get; set; }
		public string DocketNumber { get; set; }
        public string TransferStatus { get; set; }
		public decimal CancelQty { get; set; }
		public string locationCode { get; set; }
		public DateTime? ResponseDate { get; set; }
	}
}
