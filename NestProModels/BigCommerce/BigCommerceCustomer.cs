
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NestProModels
{
    public class BigCommerceCustomer
    {
        public string tax_exempt_category { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public List<FormFields> form_fields { get; set; }
    }

    public class FormFields
    {
        public string name { get; set; }

        public string value { get; set; }
    }
}
