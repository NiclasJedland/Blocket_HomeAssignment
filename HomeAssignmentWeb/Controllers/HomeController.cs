using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HomeAssignmentWeb.Models;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Web;

namespace HomeAssignmentWeb.Controllers
{
	public class HomeController : Controller
	{
		[SharePointContextFilter]
		#region Index
		public ActionResult Index()
		{
			MySession.Current.Spcontext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);
			return RedirectToAction("IndexView");
		}

		public ActionResult IndexView(string search = "")
		{
			var products = new List<Annons>();

			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				if (context != null)
				{
					var productsList = context.Web.Lists.GetByTitle("Annonser");
					var productCollection = productsList.GetItems(CamlQuery.CreateAllItemsQuery());

					context.Load(productCollection);
					context.ExecuteQuery();

					IQueryable<ListItem> searchList;
					if (string.IsNullOrEmpty(search))
						searchList = productCollection;
					else
						searchList = productCollection.Where(s => s["Rubrik"].ToString().ToLower().Contains(search)
																|| s["Text"].ToString().ToLower().Contains(search)
															);
					foreach (var item in searchList)
					{
						var user = item["User"] as FieldUserValue;
						var kategori = item["Kategori"] as TaxonomyFieldValueCollection;

						products.Add(new Annons()
						{
							Id = item["ID"].ToString(),
							Rubrik = item["Rubrik"].ToString(),
							Text = item["Text"].ToString(),
							Pris = Decimal.Parse(item["Pris"].ToString()),
							Datum = DateTime.Parse(item["Datum"].ToString()),
							Kategorier = kategori.Select(s => new Kategori() { Id = s.TermGuid, Name = s.Label }).ToList(),
							User = new Seller(user.LookupId, user.LookupValue)
						});
					}
				}
			}

			ViewBag.CurrentUser = GetUser().Id;
			return View("Index", products);
		}

		public ActionResult Search(string search = "")
		{
			return RedirectToAction("IndexView", new { search = search.ToLower().Trim() });
		}
		#endregion

		public ActionResult Details(string annonsId)
		{
			var details = new vmDetails();
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				annonsId = annonsId.Trim();
				if (context != null && !string.IsNullOrEmpty(annonsId))
				{
					var item = context.Web.Lists.GetByTitle("Annonser").GetItemById(annonsId);

					context.Load(item);
					context.ExecuteQuery();

					details.Id = item["ID"].ToString();
					details.Rubrik = item["Rubrik"].ToString();
					details.Text = item["Text"].ToString();
					details.Pris = Decimal.Parse(item["Pris"].ToString());
					details.Datum = DateTime.Parse(item["Datum"].ToString());

					var selectedKategorier = item["Kategori"] as TaxonomyFieldValueCollection;
					details.Kategorier = string.Join(", ", selectedKategorier.Select(s => s.Label).ToArray());//selectedKategorier.Select(s => s.Label).ToArray();
				}
			}
			return View(details);
		}

		#region Create
		public ActionResult CreateView()
		{
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				ViewBag.Kategorier = GetTaxonomy(context);
			}
			return View();
		}

		[HttpPost]
		public ActionResult Create(vmProductCreate product)
		{
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				if (context != null)
				{
					var user = context.Web.EnsureUser(GetUser().LoginName);

					context.Load(user);
					context.ExecuteQuery();

					var userValue = new FieldUserValue { LookupId = user.Id };

					var list = context.Web.Lists.GetByTitle("Annonser");
					var item = list.AddItem(new ListItemCreationInformation());

					item["Rubrik"] = product.Rubrik;
					item["Text"] = product.Text;
					item["Pris"] = product.Pris;
					item["Datum"] = DateTime.Now;
					item["User"] = userValue;

					item["Kategori"] = string.Join(";", product.Kategorier);

					item.Update();
					context.ExecuteQuery();
				}
			}
			return RedirectToAction("IndexView");
		}
		#endregion

		#region Delete
		public ActionResult Delete(string deleteId)
		{
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				deleteId = deleteId.Trim();
				if (context != null && !string.IsNullOrEmpty(deleteId))
				{
					var item = context.Web.Lists.GetByTitle("Annonser").GetItemById(deleteId);
					item.DeleteObject();
					context.ExecuteQuery();
				}
			}

			return RedirectToAction("IndexView");
		}
		#endregion

		#region Edit
		public ActionResult EditView(string editId)
		{
			var edit = new vmProductEdit();
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				editId = editId.Trim();
				if (context != null && !string.IsNullOrEmpty(editId))
				{
					var item = context.Web.Lists.GetByTitle("Annonser").GetItemById(editId);

					context.Load(item);
					context.ExecuteQuery();

					edit.Id = item["ID"].ToString();
					edit.Rubrik = item["Rubrik"].ToString();
					edit.Text = item["Text"].ToString();
					edit.Pris = Decimal.Parse(item["Pris"].ToString());
					edit.Datum = DateTime.Parse(item["Datum"].ToString());

					var selectedKategorier = item["Kategori"] as TaxonomyFieldValueCollection;
					edit.Kategorier = selectedKategorier.Select(s => s.TermGuid).ToArray();

					var kategorier = GetTaxonomy(context);
					ViewBag.AllaKategorier = kategorier.Select(s => new SelectListItem() { Value = s.Value, Text = s.Text }).ToList();
				}
			}
			return View(edit);
		}

		[HttpPost]
		public ActionResult Edit(vmProductEdit product)
		{
			using (var context = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
			{
				if (context != null)
				{
					var edit = context.Web.Lists.GetByTitle("Annonser").GetItemById(product.Id);

					edit["Rubrik"] = product.Rubrik;
					edit["Text"] = product.Text;
					edit["Pris"] = product.Pris;
					edit["Datum"] = DateTime.Now;
					edit["Kategori"] = string.Join(";", product.Kategorier);

					edit.Update();
					context.ExecuteQuery();
				}
			}
			return RedirectToAction("IndexView");
		}
		#endregion

		#region Misc
		public List<SelectListItem> GetTaxonomy(ClientContext context)
		{
			var items = new List<SelectListItem>();

			var taxonomy = TaxonomySession.GetTaxonomySession(context);

			var termStore = taxonomy.GetDefaultKeywordsTermStore();

			var termGroup = termStore.Groups.GetByName("Produkter");
			var termSet = termGroup.TermSets.GetByName("Datorer");

			var terms = termSet.GetAllTerms();

			context.Load(terms);
			context.ExecuteQuery();

			foreach (var term in terms)
			{
				items.Add(new SelectListItem { Text = term.Name, Value = term.Id.ToString() });
			}

			return items;
		}

		public User GetUser()
		{
			try
			{
				User user = null;
				using (var userContext = MySession.Current.Spcontext.CreateUserClientContextForSPHost())
				{
					user = userContext.Web.CurrentUser;
					userContext.Load(user);
					userContext.ExecuteQuery();
				}
				return user;
			}
			catch (Exception e)
			{
				throw new HttpException("Error: " + e.Message);
			}
		}

		public string GetTermName(ClientContext context, Guid termId)
		{
			var taxonomy = TaxonomySession.GetTaxonomySession(context);

			var termStore = taxonomy.GetDefaultKeywordsTermStore();

			var termGroup = termStore.Groups.GetByName("Produkter");
			var termSet = termGroup.TermSets.GetByName("Datorer");

			var terms = termSet.GetAllTerms();

			context.Load(terms);
			context.ExecuteQuery();

			var term = terms.Where(s => s.Id == termId);
			return term.Select(s => s.Name).ToString();
		}
		#endregion
	}
}
