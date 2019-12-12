using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly ILogger<FibonacciController> _logger;

        public FibonacciController(ILogger<FibonacciController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public int Get(int x)
        {

             if (x < 0) throw new ArgumentNullException(
                 "Must be at leat 0", nameof(x));

            // return Fib(x).current;

             (int current, int previous) = Fib(x);
             return previous;

            (int current, int previous) Fib(int i) {
                    if (i == 0) return (0, 1);      

                    var (current, previous) = Fib(i - 1);

                    return (current + previous, current);
             }


        }
        
    }
}
