using System;
using System.Collections.Generic;

namespace PT.Domain.Model
{
    public class IssuerTypeItem
    {
        public int issuerTypeId { get; set; }
        public string issuerTypeName { get; set; }
    }

    public class IssuerTypesResponse
    {
        public bool success { get; set; }
        public List<IssuerTypeItem> data { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        public DateTime? timestamp { get; set; }
    }

    public class OpinionTypeItem
    {
        public int opinionTypeId { get; set; }
        public string opinionTypeName { get; set; }
    }

    public class OpinionTypesResponse
    {
        public bool success { get; set; }
        public List<OpinionTypeItem> data { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        public DateTime? timestamp { get; set; }
    }
}