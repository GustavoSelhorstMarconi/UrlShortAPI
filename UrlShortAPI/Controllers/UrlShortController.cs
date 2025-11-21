using Microsoft.AspNetCore.Mvc;
using UrlShort.Application.Dto;
using UrlShort.Application.Interfaces;

namespace UrlShort.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UrlShortController : ControllerBase
    {
        private readonly IUrlShortService _urlShortService;
        
        public UrlShortController(IUrlShortService urlShortService)
        {
            _urlShortService = urlShortService;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] ShortenUrlDto shortenUrlDto)
        {
            var response = await _urlShortService.ShortenUrl(shortenUrlDto);
            
            if (response.StatusCode is >= 200 and < 300)
                return Ok(response);
            else if (response.StatusCode is >= 400 and < 500)
                return BadRequest(response);
            else
                return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> Get(string shortUrl)
        {
            var response = await _urlShortService.GetShortUrl(shortUrl);
            
            if (response.StatusCode is >= 200 and < 300)
                return Redirect(response.Data);
            else if (response.StatusCode is >= 400 and < 500)
                return BadRequest(response);
            else
                return StatusCode(response.StatusCode, response);
        }
    }
}
