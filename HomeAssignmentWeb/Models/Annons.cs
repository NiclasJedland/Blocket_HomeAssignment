using System;
using System.Collections.Generic;

namespace HomeAssignmentWeb.Models
{
	public class Annons
	{
		public string Id { get; set; }
		public string Rubrik { get; set; }
		public string Text { get; set; }
		public decimal Pris { get; set; }
		public DateTime Datum { get; set; }
		public Seller User { get; set; }
		public List<Kategori> Kategorier { get; set; }
	}
}