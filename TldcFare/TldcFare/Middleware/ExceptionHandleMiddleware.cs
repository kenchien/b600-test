using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.WebApi.IService;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Middleware {
    public class ExceptionHandleMiddleware {
        private RequestDelegate _next { get; }

        public ExceptionHandleMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IExceptionLogService logService, IWebHostEnvironment _env) {
            try {
                await _next(context);
            } catch (Exception ex) {
                await HandleExceptionAsync(context, ex, logService, _env);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex, IExceptionLogService _logService, IWebHostEnvironment _env) {
            ApiFailModel apiFail;
            try {
                var innerMessage = string.Empty;
                var ie = ex.InnerException;
                if (ie != null) {
                    innerMessage = $"The Inner Exception:\nException Name: {ie.GetType().Name}\nMessage:\n{ie.Message}";
                }

                var entry = new Logofexception() {
                    OperId = context.User.FindFirst(ClaimTypes.NameIdentifier) == null
                            ? ""
                            : context.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                                    ExceptionType = ex.GetType().ToString(), //錯誤分類
                                    Params = ex.Message,                     //真正的錯誤訊息(有時候會多包一層到InnerException)
                                    ExceptionContent = ex.StackTrace,        //沒有錯誤訊息,只有簡短的傳拋過程
                                    InnerException = innerMessage            //有時候會多包一層
                };

                if (!_logService.ExLogIns(entry)) {
                    var temp2 = new ApiFailModel() {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "發生錯誤,連寫入logOfException都失敗"
                    };
                    apiFail = temp2;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = apiFail.StatusCode;
                    return context.Response.WriteAsync(apiFail.ToString());
                }

                var exceptionType = ex.GetType();
                apiFail = new ApiFailModel();
                if (exceptionType == typeof(UnauthorizedAccessException)) {
                    apiFail.StatusCode = StatusCodes.Status401Unauthorized;
                    apiFail.SystemMessage = ex.Message;
                } else if (exceptionType == typeof(NotImplementedException)) {
                    apiFail.StatusCode = StatusCodes.Status400BadRequest;
                    apiFail.SystemMessage = ex.Message;
                } else {
                    apiFail.StatusCode = StatusCodes.Status400BadRequest;
                    apiFail.SystemMessage = ex.Message;
                }

                if (_env.IsDevelopment() || exceptionType == typeof(CustomException)) {
                    apiFail.Message = ex.Message;
                } else {
                    apiFail.Message = "執行失敗";
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = apiFail.StatusCode;

            } catch (Exception handleErr) {
                var temp = new ApiFailModel() {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $@"HandleExceptionAsync發生錯誤,{handleErr.Message}"
                };
                apiFail = temp;
            } finally {

            }
            return context.Response.WriteAsync(apiFail.ToString());
        }
    }
}