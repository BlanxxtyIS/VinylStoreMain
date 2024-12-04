using Microsoft.AspNetCore.Mvc;
using VinylShop.Api.Service;
using VinylShop.Shared.Models;

namespace VinylShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumIterationController : ControllerBase
    {
        private readonly AlbumService _albumService;

        public AlbumIterationController(AlbumService albumService)
        {
            _albumService = albumService;
        }


        [HttpGet]
        [Route("getAlbums")]
        public async Task<ActionResult<List<Album>>> GetAlbums()
        {
            return await _albumService.GetAllAlbumAsync();
        }

        [HttpGet]
        [Route("getAlbumId/{id}")]
        public async Task<ActionResult<Album>> GetAlbum(int id)
        {
            var album = await _albumService.GetAlbumAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            return album;
        }

        [HttpPost]
        [Route("createAlbum")]
        public async Task<ActionResult<Album>> CreateAlbum(Album album)
        {
            await _albumService.AddAlbumAsync(album);
            return CreatedAtAction(nameof(GetAlbum), new { id = album.AlbumId }, album);
        }

        [HttpPut]
        [Route("updateAlbym/{id}")]
        public async Task<IActionResult> UpdateAlbum(int id, Album album)
        {
            if (id != album.AlbumId)
            {
                return BadRequest();
            }

            try
            {
                await _albumService.UpdateAlbumAsync(album);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        
        [HttpDelete]
        [Route("deleteAlbum/{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            try
            {
                await _albumService.DeleteAlbumAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
