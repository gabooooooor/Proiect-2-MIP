using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindowsFormsApp6
{
    [Table("IstoricVanzari")]
    public class IstoricVanzari
    {
        [Key]
        public int Id { get; set; }

        public int IdProdus { get; set; }
        public int Cantitate { get; set; }

        // salvam si denumirea si categoria, deoarece produsul se sterge cand stocul ajunge la 0
        public string DenumireProdus { get; set; }
        public int IdCategorie { get; set; }
        public DateTime DataVanzarii { get; set; }
    }
}