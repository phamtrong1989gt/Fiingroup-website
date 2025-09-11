using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public  interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {

    }

    public class PaymentTransactionRepository : BaseRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

