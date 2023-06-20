using System;
using System.Collections.Generic;
using System.Text;

namespace AwsSqsServiceAstha.QResponse
{
    public class NewData
    {
        public string Availability { get; set; }
        public string productSKU { get; set; }
        public string productId { get; set; }
        public string variantProductId { get; set; }
        public string productURL { get; set; }
        public string variantSKU { get; set; }
    }
    public class ProductInfo
    {
        public string Sku { get; set; }
        public string VariantSku { get; set; }
        public bool IsEc { get; set; }       
    }

    public class ProductResponse
    {
        public string action { get; set; }
        public NewData newData { get; set; }
        public List<ProductInfo> products { get; set; }
    }
}
