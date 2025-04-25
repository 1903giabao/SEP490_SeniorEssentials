using Firebase.Auth.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SE.Common.Setting;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface IJwtService
    {
        Task<TokenResponse> GenerateTokens(Account account, string ipAddress);
        Task<TokenResponse> RefreshToken(string token, string ipAddress);
        Task RevokeToken(string token, string ipAddress);
        public string CreateJwtToken(Account user);
    }

    public class JwtService : IJwtService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private static readonly List<Common.Setting.RefreshToken> _refreshTokens = new();

        public JwtService(UnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<TokenResponse> GenerateTokens(Account account, string ipAddress)
        {
            var jwtToken = CreateJwtToken(account);
            var refreshToken = await GenerateRefreshToken(account.AccountId, ipAddress);

            return new TokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token,
            };
        }

        public async Task<TokenResponse> RefreshToken(string token, string ipAddress)
        {
            var refreshToken = await GetRefreshToken(token);
            if (refreshToken == null || !refreshToken.IsActive)
                throw new SecurityTokenException("Invalid refresh token");

            await RevokeRefreshToken(token, ipAddress, "Refreshed token");

            var account = _unitOfWork.AccountRepository
                .FindByCondition(a => a.AccountId == refreshToken.AccountId)
                .FirstOrDefault();

            return await GenerateTokens(account, ipAddress);
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
            await RevokeRefreshToken(token, ipAddress);
        }

        public string CreateJwtToken(Account user)
        {
            var userRole = _unitOfWork.RoleRepository.FindByCondition(u => u.RoleId == user.RoleId).FirstOrDefault();
            var isInformation = _unitOfWork.AccountRepository
                .FindByCondition(u => u.FullName != null && u.Email.Equals(user.Email))
                .FirstOrDefault();

            var authClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, user.AccountId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.PhoneNumber),
                new(ClaimTypes.Role, userRole.RoleName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Name, user.FullName ?? "null"),
                new("Avatar", user.Avatar ?? "null"),
                new("IsInformation", isInformation != null ? "true" : "false")
            };

            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JwtSettings"));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public Task<Common.Setting.RefreshToken> GenerateRefreshToken(int accountId, string ipAddress)
        {
            _refreshTokens.RemoveAll(x => x.AccountId == accountId);

            var refreshToken = new Common.Setting.RefreshToken
            {
                AccountId = accountId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(30),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _refreshTokens.Add(refreshToken);
            return Task.FromResult(refreshToken);
        }

        public Task<Common.Setting.RefreshToken> GetRefreshToken(string token)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
            return Task.FromResult(refreshToken);
        }

        public Task RevokeRefreshToken(string token, string ipAddress, string reason = null)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
            if (refreshToken != null)
            {
                refreshToken.Expires = DateTime.UtcNow;
                refreshToken.CreatedByIp = reason ?? ipAddress;
            }
            return Task.CompletedTask;
        }
    }
}
