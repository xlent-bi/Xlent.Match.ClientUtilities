namespace ClientAdapter.Business
{
    public class Customer
    {
        private static Storage<Customer> _storage = new Storage<Customer>();

        public Model.Customer Model { get; private set; }

        public static Customer Get(int id)
        {
            var customer = _storage.GetData(id);
            return customer;
        }

        public static Customer Create()
        {
            int id = _storage.NextId();

            return new Customer()
            {
                Model = new Model.Customer(id)
            };
        }
    }
}
