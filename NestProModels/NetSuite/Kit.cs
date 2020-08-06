using System;
using System.Collections.Generic;
using System.Text;

namespace NestProModels
{
    public class Kit
    {
        public Kit(int kitId)
        {
            ID = kitId;
        }

        public int ID { get; set; }

        public int TotalItemQuantity { get; set; }
    }
}