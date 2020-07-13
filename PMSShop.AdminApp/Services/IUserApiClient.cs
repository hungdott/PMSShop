using PMSShop.ViewModels.Common;
using PMSShop.ViewModels.System.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMSShop.AdminApp.Services
{
    public interface IUserApiClient
    {
        Task<string> Authenticate(LoginRequest request);

        Task<PagedResult<UserViewModel>> GetUsersPaging(GetUserPagingRequest request);
    }
}