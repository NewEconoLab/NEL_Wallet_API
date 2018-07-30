using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NEL_Wallet_API.Controllers
{
    [Route("api/[controller]")]
    public class TestWebController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        // POST api/<controller>
        [HttpPost]
        public string Post()
        {
            return "TestWebController.Post.res:value1+value2";
        }
    }
}
