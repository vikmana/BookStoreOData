//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BookStore.Models;
//using Microsoft.AspNet.OData;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace BookStore.Controllers
//{
//    [Route("api/[controller]")] 
//    [ApiController]
//    [ApiExplorerSettings(IgnoreApi = false)]
//    public class PressesController : ODataController
//    {
//        private BookStoreContext _db;

//        public PressesController(BookStoreContext context)
//        {
//            _db = context;
//            if (context.Presses.Any()) return;

//            foreach (var b in DataSource.GetPresses())
//            {
//                context.Presses.Add(b);
//                context.Presses.Add(b.Press);
//            }
//            context.SaveChanges();
//        }

//        [HttpGet]
//        [EnableQuery]
//        public IActionResult Get()
//        {
//            return Ok(_db.Presses);
//        }

//        [HttpGet("{key}")]
//        [EnableQuery]
//        public IActionResult Get(int key)
//        {
//            return Ok(_db.Presses.FirstOrDefault(c => c.Id == key));
//        }
//    }

//}