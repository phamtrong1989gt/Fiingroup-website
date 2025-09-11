using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class TourDayGallery : IAggregateRoot
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int TourDayId { get; set; }
        public string Name { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public bool Status { get; set; }
        public string Language { get; set; }
    }
}
