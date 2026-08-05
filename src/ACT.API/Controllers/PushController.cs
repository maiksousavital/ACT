using ACT.API.Extensions;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PushController : ControllerBase
{
    private readonly IPushSubscriptionService _service;
    private readonly IConfiguration _config;

    public PushController(IPushSubscriptionService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }

    /// <summary>
    /// Public — this is the VAPID *public* key, meant to be handed to the browser's
    /// PushManager.subscribe(). It isn't sensitive; only the private key is.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey()
    {
        var publicKey = _config["VapidSettings:PublicKey"];
        if (string.IsNullOrWhiteSpace(publicKey))
            return NotFound(new { message = "Push notifications are not configured on this server." });

        return Ok(new { publicKey });
    }

    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribePushRequest request)
    {
        var userId = User.GetUserId()!.Value;
        await _service.SubscribeAsync(userId, request);
        return NoContent();
    }

    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribePushRequest request)
    {
        await _service.UnsubscribeAsync(request.Endpoint);
        return NoContent();
    }
}
