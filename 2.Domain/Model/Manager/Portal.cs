using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class Portal : IAggregateRoot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}
