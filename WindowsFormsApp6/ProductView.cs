using System;

namespace WindowsFormsApp6
{
    public class ProductView
    {
        public int Id { get; set; }
        public string Denumire { get; set; }
        public string Descriere { get; set; }
        public string Categorie { get; set; }
        public int IdCategorie { get; set; }
        public DateTime DataIntrarii { get; set; }
        public DateTime DataExpirarii { get; set; }
        public int Cantitate { get; set; }
    }
}