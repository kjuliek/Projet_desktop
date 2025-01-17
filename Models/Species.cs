using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_desktop.Models
{
    internal class Species
    {
        public string       Name          { get; set; }
        public int          PurchasePrice { get; set; }
        public ProductType  ProductType   { get; set; }
        public BuildingType BuildingType  { get; set; }

        public Species()
        {
            Name         = "";
            ProductType  = new();
            BuildingType = new();
        }
        public Species(string name, int purchase_price, ProductType product_type, BuildingType building_type)
        {
            Name          = name;
            PurchasePrice = purchase_price;
            ProductType   = product_type;
            BuildingType  = building_type;
        }
    }
}
