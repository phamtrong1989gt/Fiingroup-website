using Newtonsoft.Json;

namespace PT.Domain.Model
{
    public class DeleteNewsRequest
    {
        [JsonProperty("NewsId")]
        public int NewsId { get; set; }

        [JsonProperty("DeleteBy")]
        public string DeleteBy { get; set; }
    }
}
