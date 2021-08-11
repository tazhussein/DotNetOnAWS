using Microsoft.AspNetCore.Mvc;
using WelcomeMessage.API.Services;

namespace WelcomeMessage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WelcomeMessageController : ControllerBase
    {

        [HttpPost]
        public IActionResult Post([FromBody] string userName)
        {
            string retval = string.Empty;

            WelcomeMessageSvc svc = new WelcomeMessageSvc();
            retval = svc.GetWelcomeMessage(userName);

            if (retval != string.Empty)
            {
                return Ok(retval);
            }
            else
            {
                return NoContent();
            }

        }
    }
}
