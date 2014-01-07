namespace ClientAdapter.Model
{
    public class Customer : BaseClass
    {
        public string CustomerNumber { get; set; }
        public int PersonId { get; set; }

        public Customer(int id)
            : base(id)
        { }

        public override string ToString()
        {
            return string.Format("{0}", CustomerNumber);
        }
    }
}
