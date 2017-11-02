using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HomeAssignmentWeb.Models
{
	public class vmProductEdit
	{
		public string Id { get; set; }
		public string Rubrik { get; set; }
		public string Text { get; set; }
		public decimal Pris { get; set; }
		public DateTime Datum { get; set; }
		public Seller User { get; set; }
		public string[] Kategorier { get; set; }

	}
}