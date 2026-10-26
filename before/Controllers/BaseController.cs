using Library.Before.Data;
using Microsoft.AspNetCore.Mvc;

namespace Library.Before.Controllers
{
    public class BaseController : ControllerBase
    {
        private LibraryContext _db;

        protected LibraryContext Db
        {
            get
            {
                if (_db == null)
                    _db = new LibraryContext();
                return _db;
            }
        }
    }
}
