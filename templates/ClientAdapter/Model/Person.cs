namespace ClientAdapter.Model
{
    public class Person : BaseClass
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Person(int id)
            : base(id)
        { }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", FirstName, LastName, Id);
        }
    }
}
