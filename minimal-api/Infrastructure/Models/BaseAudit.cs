namespace minimal_api.Infrastructure.Models
{
    public class BaseAudit
    {
        public Guid Id { get; private set; }
        public bool IsCanceled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        //new object defaults
        public BaseAudit()
        {
            Id = Guid.NewGuid();
            IsCanceled = false;
        }
    }
}
