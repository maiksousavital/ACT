using ACT.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/followup-periods")]
public class FollowUpPeriodsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetAll()
    {
        var values = Enum.GetValues(typeof(FollowUpPeriod))
            .Cast<FollowUpPeriod>()
            .Select(p => new { Name = p.ToString(), Days = (int)p })
            .ToList();
        return Ok(values);
    }
}
