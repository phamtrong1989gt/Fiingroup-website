using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PT.Domain.Model.Misa
{
    #region Token Models

    /// <summary>
    /// MISA API Token Request Model
    /// </summary>
    public class MisaTokenRequest
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }

    /// <summary>
    /// MISA API Token Response Model
    /// </summary>
    public class MisaTokenResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }

    #endregion

    #region Contact Models

    /// <summary>
    /// MISA Contact Model for Creating Contact
    /// </summary>
    public class MisaContactRequest
    {
        [JsonProperty("form_layout")]
        public string FormLayout { get; set; }

        [JsonProperty("contact_code")]
        public string ContactCode { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("contact_name")]
        public string ContactName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("office_email")]
        public string OfficeEmail { get; set; }

        [JsonProperty("office_tel")]
        public string OfficeTel { get; set; }

        [JsonProperty("customer_since_date")]
        public string CustomerSinceDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }
    }

    /// <summary>
    /// MISA Contact Creation Response Model
    /// </summary>
    public class MisaContactResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("results")]
        public List<MisaContactResult> Results { get; set; }
    }

    /// <summary>
    /// MISA Contact Creation Result Item
    /// </summary>
    public class MisaContactResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public int Data { get; set; }
    }

    #endregion
}
