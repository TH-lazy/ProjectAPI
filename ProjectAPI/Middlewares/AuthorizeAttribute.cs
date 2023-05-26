using ProjectAPI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ProjectAPI.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var _itemsDataContext = context.HttpContext.Items;

                if (_itemsDataContext["StatusToken"] == null)
                {
                    context.Result = new JsonResult(
                        new ResponseData()
                        {
                            StatusCode = StatusCodes.Status403Forbidden,
                            StatusMessage = ConstantsMessage.FORBIDDEN,
                        })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
                else
                {
                    switch (_itemsDataContext["StatusToken"])
                    {
                        case nameof(ConstantsMessage.TOKEN_EXPIRED):
                            context.Result = new JsonResult(
                                new ResponseData()
                                {
                                    StatusCode = StatusCodes.Status401Unauthorized,
                                    StatusMessage = _itemsDataContext["MessageToken"].ToString(),
                                })
                            {
                                StatusCode = StatusCodes.Status401Unauthorized
                            };
                            break;
                        case nameof(ConstantsMessage.UNAUTHORIZED):
                            context.Result = new JsonResult(
                                new ResponseData()
                                {
                                    StatusCode = StatusCodes.Status401Unauthorized,
                                    StatusMessage = _itemsDataContext["MessageToken"].ToString(),
                                })
                            {
                                StatusCode = StatusCodes.Status401Unauthorized
                            };
                            break;
                        case nameof(ConstantsMessage.TOKEN_INVALID):
                            context.Result = new JsonResult(
                                new ResponseData()
                                {
                                    StatusCode = StatusCodes.Status401Unauthorized,
                                    StatusMessage = _itemsDataContext["MessageToken"].ToString(),
                                })
                            {
                                StatusCode = StatusCodes.Status401Unauthorized
                            };
                            break;
                        case nameof(ConstantsMessage.NODATA):
                            context.Result = new JsonResult(
                                new ResponseData()
                                {
                                    StatusCode = StatusCodes.Status204NoContent,
                                    StatusMessage = _itemsDataContext["MessageToken"].ToString(),
                                })
                            {
                                StatusCode = StatusCodes.Status204NoContent
                            };
                            break;
                        case nameof(ConstantsMessage.BADREQUEST):
                            context.Result = new JsonResult(
                                new ResponseData()
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    StatusMessage = _itemsDataContext["MessageToken"].ToString(),
                                })
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}