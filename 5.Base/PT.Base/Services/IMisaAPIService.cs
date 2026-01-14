using System.Threading.Tasks;
using PT.Domain.Model.Misa;

namespace PT.Base.Services
{
    /// <summary>
    /// MISA CRM API Service Interface
    /// Provides methods to interact with MISA CRM API for token generation and contact creation
    /// </summary>
    public interface IMisaAPIService
    {
        /// <summary>
        /// Get access token from MISA API
        /// </summary>
        /// <param name="clearCache">Whether to clear cached token and get a new one</param>
        /// <returns>Access token string</returns>
        Task<string> GetAccessTokenAsync(bool clearCache = false);

        /// <summary>
        /// Create a new contact in MISA CRM
        /// </summary>
        /// <param name="contact">Contact information</param>
        /// <returns>MISA contact creation response</returns>
        Task<MisaContactResponse> CreateContactAsync(MisaContactRequest contact);

        /// <summary>
        /// Create multiple contacts in MISA CRM
        /// </summary>
        /// <param name="contacts">List of contact information</param>
        /// <returns>MISA contact creation response</returns>
        Task<MisaContactResponse> CreateContactsAsync(MisaContactRequest[] contacts);
    }
}
