using System.Web;

namespace HomeAssignmentWeb.Models
{
	public class MySession
	{
		public SharePointContext Spcontext { get; set; }
		public static MySession Current
		{
			get
			{
				MySession session = (MySession)HttpContext.Current.Session["_MySession_"];
				if (session == null)
				{
					session = new MySession();
					HttpContext.Current.Session["_MySession_"] = session;
				}
				return session;
			}
		}
	}
}