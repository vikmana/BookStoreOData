using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace BookStore.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class BooksController : ODataController
    {
        private BookStoreContext _db;

        public BooksController(BookStoreContext context)
        {
            _db = context;
            if (context.Books.Any()) return;

            foreach (var b in DataSource.GetBooks())
            {
                context.Books.Add(b);
                context.Presses.Add(b.Press);
            }
            context.SaveChanges();
        }

        [HttpGet]
        [EnableQuery(PageSize = 5)]
        public IActionResult Get()
        {
            return Ok(_db.Books);
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_db.Books.FirstOrDefault(c => c.Id == key));
        }
    }

}