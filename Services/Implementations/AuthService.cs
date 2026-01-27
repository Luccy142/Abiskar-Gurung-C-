using System.Security.Cryptography;
using System.Text;
using JournalApp.Data.Database;
using JournalApp.Models.Entities;
using JournalApp.Services.Interfaces;
using SQLite;

namespace JournalApp.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly SQLiteAsyncConnection _db;

        public AuthService(AppDatabase database)
        {
            _db = database.Connection;
        }

        public async Task<bool> IsPinSetAsync()
        {
            var row = await _db.Table<UserSecurity>().Where(x => x.Id == 1).FirstOrDefaultAsync();
            return row != null && !string.IsNullOrWhiteSpace(row.PinHash);
        }

        public async Task<bool> SetPinAsync(string pin)
        {
            pin = (pin ?? string.Empty).Trim();

            if (pin.Length < 4 || pin.Length > 8) return false;
            if (!pin.All(char.IsDigit)) return false;

            var hash = HashPin(pin);

            var existing = await _db.Table<UserSecurity>().Where(x => x.Id == 1).FirstOrDefaultAsync();
            if (existing == null)
            {
                var record = new UserSecurity
                {
                    Id = 1,
                    PinHash = hash,
                    CreatedAt = DateTime.Now
                };
                await _db.InsertAsync(record);
            }
            else
            {
                existing.PinHash = hash;
                await _db.UpdateAsync(existing);
            }

            return true;
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            pin = (pin ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pin)) return false;

            var existing = await _db.Table<UserSecurity>().Where(x => x.Id == 1).FirstOrDefaultAsync();
            if (existing == null || string.IsNullOrWhiteSpace(existing.PinHash)) return false;

            return HashPin(pin) == existing.PinHash;
        }

        private static string HashPin(string pin)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pin);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
