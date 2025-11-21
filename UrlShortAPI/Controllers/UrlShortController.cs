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

        /// <summary>
        ///     Cria um URL encurtado
        /// </summary>
        /// <param name="shortenUrlDto">Dados da URL a ser encurtada</param>
        /// <returns>Dados da URL encurtada criada</returns>
        /// <response code="200">URL encurtada criada com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UrlShortDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UrlShortDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UrlShortDto>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        ///     Obtém uma URL original pela URL encurtada
        /// </summary>
        /// <param name="hashUrl">Hash da URL</param>
        /// <returns>URL original da URL encurtada</returns>
        /// <response code="302">Redireciona para a URL original</response>
        /// <response code="400">Dados inválidos</response>
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [HttpGet("{hashUrl}")]
        public async Task<IActionResult> Get(string hashUrl)
        {
            var response = await _urlShortService.GetShortUrl(hashUrl);
            
            if (response.StatusCode is >= 200 and < 300)
                return Redirect(response.Data);
            else if (response.StatusCode is >= 400 and < 500)
                return BadRequest(response);
            else
                return StatusCode(response.StatusCode, response);
        }
    }
}
