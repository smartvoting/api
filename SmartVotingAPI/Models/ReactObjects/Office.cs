namespace SmartVotingAPI.Models.ReactObjects
{
    public class Office
    {
        public string Type { get; set; }
        public string? StreetNumber { get; set; }
        public string? StreetName { get; set; }
        public string? UnitNumber { get; set; }
        public string City { get; set; } = null!;
        public string? Province { get; set; }
        public string PostCode { get; set; } = null!;
        public string? PoBox { get; set; }
        public bool IsPublic { get; set; }

        //public string GetAddress(string? number, string? name, string? unit, string city, string province, string postCode, string? poBox)
        //{
        //    string address = "";

        //    if (!String.IsNullOrEmpty(poBox))
        //        address += (poBox + "\n");

        //    if (!String.IsNullOrEmpty(number) && !String.IsNullOrEmpty(name))
        //        address += (number + " " + name + "\n");

        //    if (!String.IsNullOrEmpty(unit))
        //        address += ("Suite " + unit + "\n");

        //    address += (city + ", " + province + " " + postCode);

        //    return address;
        //}
    }
}
