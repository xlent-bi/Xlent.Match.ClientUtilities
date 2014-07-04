namespace Crm.ClientAdapter.Business
{
    public class Person
    {
        private static Storage<Person> _storage = new Storage<Person>();

        public Model.Person Model { get; private set; }

        public static Person Get(int id)
        {
            return _storage.GetData(id);
        }

        public static Person Create()
        {
            int id = _storage.NextId();

            return new Person()
            {
                Model = new Model.Person(id)
            };
        }
    }
}
