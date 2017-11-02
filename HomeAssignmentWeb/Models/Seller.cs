using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeAssignmentWeb.Models
{
	public class Seller
	{
		public Seller(int id, string name)
		{
			Id = id;
			Name = name;
		}
		public int Id { get; set; }
		public string Name { get; set; }
	}
}