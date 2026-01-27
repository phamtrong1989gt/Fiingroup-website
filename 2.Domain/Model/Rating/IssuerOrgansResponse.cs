using System;
using System.Collections.Generic;

namespace PT.Domain.Model
{
    public class IssuerOrgansResponse
    {
        public bool Success { get; set; }
        public IssuerOrgansData Data { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class IssuerOrgansData
    {
        public List<IssuerOrganItem> Items { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class IssuerOrganItem
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string IndustryTypeName { get; set; }
    }
}
