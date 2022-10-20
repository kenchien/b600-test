using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TldcFare.Dal.Common;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.IService;

namespace TldcFare.WebApi.Middleware {
   public class UserActLogMiddleware {
      private readonly RequestDelegate _next;

      public UserActLogMiddleware(RequestDelegate next) {
         _next = next;
      }

      public async Task Invoke(HttpContext context, IAdminService admin, IOperService userService, JwtHelper jwt) {
         try {
            var sw = new Stopwatch();
            sw.Start();

            #region 官網api 權限管控

            context.Request.Headers.TryGetValue("Authorization", out var token);
            if (!string.IsNullOrEmpty(token)) {
               // token = token.ToString().Substring("Bearer ".Length);
               // var tokenHandler = new JwtSecurityTokenHandler();
               // JwtSecurityToken securityToken = tokenHandler.ReadJwtToken(token);
               // securityToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
               var role = jwt.GetOperGrpId();
               var funcId = context.Request.Path.ToString().Split('/')[3];
               var funcParent = context.Request.Path.ToString().Split('/')[2];

               if (funcParent.ToUpper() == "ADMIN" || funcParent.ToUpper() == "SYSTEMSETTING") {
                  if (role.ToUpper() != "ADMIN") throw new CustomException("沒有此功能權限, 請聯絡系統管理員");
               }

               if (role == "Sev") //官網api 做權限檢查
               {
                  var authDetail = userService.CheckAuth(role, funcId);
                  if (string.IsNullOrEmpty(authDetail)) {
                     throw new UnauthorizedAccessException();
                  }
               }

               //HandleOperLog(context, securityToken, 0, userService);
            }

            #endregion

            await _next(context);
            sw.Stop();
         } catch (Exception ex) {
            throw ex;
         }
      }
   }
}