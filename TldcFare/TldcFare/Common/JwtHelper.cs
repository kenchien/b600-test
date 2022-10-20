using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TldcFare.WebApi.Common
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string JwtToken(string operId, string operGrpId, int expires = 600)
        {
            try
            {
                var issuer = _configuration.GetValue<string>("Jwt:Issuer");
                var signKey = _configuration.GetValue<string>("Jwt:Key");

                // 設定要加入到 JWT Token 中的聲明資訊(Claims)
                var claims = new List<Claim>();

                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, operId)); // User.Identity.Name
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID
                claims.Add(new Claim("role", operGrpId)); // JWT role

                var userClaimsIdentity = new ClaimsIdentity(claims);

                // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

                // HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                // 建立 SecurityTokenDescriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Subject = userClaimsIdentity,
                    Expires = DateTime.Now.AddMinutes(expires),
                    SigningCredentials = signingCredentials
                };

                // 產出所需要的 JWT securityToken 物件，並取得序列化後的 Token 結果(字串格式)
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var serializeToken = tokenHandler.WriteToken(securityToken);

                return serializeToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get operGrpId from _jwt
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public string GetOperGrpId()
        {
            string role = string.Empty;
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            if (!string.IsNullOrEmpty(token))
            {
                if (token == "Bearer") throw new UnauthorizedAccessException("登入逾時");

                token = token.ToString().Substring("Bearer ".Length);
                var tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken securityToken = tokenHandler.ReadJwtToken(token);
                role = securityToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            }

            return role;
        }

        /// <summary>
        /// 從jwt 取 operId
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public string GetOperIdFromJwt()
        {
            var operId = string.Empty;
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            if (string.IsNullOrEmpty(token)) return operId;
            
            if (token == "Bearer") throw new UnauthorizedAccessException("登入逾時");

            token = token.ToString().Substring("Bearer ".Length);
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);
            operId = securityToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return operId;
        }

        /// <summary>
        /// 解密函數
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string DecryptAes(string text)
        {
            const string key = "2019111191912366";
            const string iv = "2019111191912366";

            var encryptBytes = System.Convert.FromBase64String(text);
            var aes = System.Security.Cryptography.Aes.Create();
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
            aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);
            var transform = aes.CreateDecryptor();
            return System.Text.Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0,
                encryptBytes.Length));
        }
        
        public string HashPwd(string pwd)
        {
            var saltBytes = Encoding.UTF8.GetBytes(DecryptAes(_configuration["pwdsalt"]));

            // derive a 256-bit subkey 
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: pwd,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }

    }
}