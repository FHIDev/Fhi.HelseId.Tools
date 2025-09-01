namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// Configuration options for HelseId Selvbetjening API
    /// </summary>
    public class SelvbetjeningConfiguration
    {
        /// <summary>
        /// HelseId authority address (e.g., https://helseid-sts.test.nhn.no)
        /// </summary>
        public required string Authority { get; set; }
        /// <summary>
        /// Base address for HelseId Selvbetjening API
        /// </summary>
        public required string BaseAddress { get; set; }
    }
}
