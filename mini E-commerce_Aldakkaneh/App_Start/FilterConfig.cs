using System.Web;
using System.Web.Mvc;

namespace mini_E_commerce_Aldakkaneh
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
