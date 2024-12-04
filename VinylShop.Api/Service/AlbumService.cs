using Microsoft.EntityFrameworkCore;
using VinylShop.Api.Persistence;
using VinylShop.Shared.Models;
namespace VinylShop.Api.Service
{
    public class AlbumService
    {
        private readonly VinylShopContext _context;

        public AlbumService(VinylShopContext context)
        {
            _context = context;
        }

        //для получения всех
        public async Task<List<Album>> GetAllAlbumAsync()
        {
            return await _context.Albums.ToListAsync();
        }

        //для получения по id
        public async Task<Album> GetAlbumAsync(int id)
        {
            return await _context.Albums.FirstOrDefaultAsync(a => a.AlbumId == id);
        }

        //для добавления нового альбома
        public async Task AddAlbumAsync(Album album)
        {
            await _context.Albums.AddAsync(album);
            await _context.SaveChangesAsync();
        }

        //для обновления существующего 
        public async Task UpdateAlbumAsync(Album album)
        {
            var existingAlbum = await GetAlbumAsync(album.AlbumId);
            if (existingAlbum != null)
            {
                existingAlbum.Name = album.Name;
                existingAlbum.Description = album.Description;
                existingAlbum.Tracks = album.Tracks;

                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Альбом не найден");
            }
        }

        //Удалить по id
        public async Task DeleteAlbumAsync(int id)
        {
            var albumDelete = await GetAlbumAsync(id);
            if (albumDelete != null)
            {
                _context.Albums.Remove(albumDelete);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Альбом не найден");
            }
        }

    }
}
