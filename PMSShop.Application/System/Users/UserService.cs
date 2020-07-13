using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PMSShop.Application.Common;
using PMSShop.Data.Entities;
using PMSShop.Utilities.Constants;
using PMSShop.Utilities.Exceptions;
using PMSShop.ViewModels.Common;
using PMSShop.ViewModels.System.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PMSShop.Application.System.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManage;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        private const string USER_CONTENT_FOLDER_NAME = "user-content";

        public UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IConfiguration config, IStorageService storageService)
        {
            _userManage = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _storageService = storageService;
        }

        public async Task<string> Authencate(LoginRequest request)
        {
            var user = await _userManage.FindByNameAsync(request.Username);
            if (user == null) return null;
            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
            if (!result.Succeeded)
            {
                return null;
            }
            var roles = _userManage.GetRolesAsync(user);
            var pathAvartar = "";
            if (!String.IsNullOrEmpty(user.Avatar))
            {
                pathAvartar = SystemContants.HostAPI + _storageService.GetFileUrl(user.Avatar);
                // claims.ToList().Add(new Claim("avatar", pathAvartar));
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.GivenName,user.FirstName),
                new Claim(ClaimTypes.Role,string.Join(";",roles)),
                new Claim(ClaimTypes.Name,request.Username),
                new Claim("avatar",pathAvartar)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                                             _config["Tokens:Issuer"],
                                             claims,
                                             expires: DateTime.Now.AddHours(3),
                                             signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var user = new AppUser()
            {
                Dob = request.Dob,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName
            };
            if (request.Avatar != null)
            {
                user.Avatar = await this.SaveFile(request.Avatar);
                user.FileSize = request.Avatar.Length;
            }
            var result = await _userManage.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }

        public async Task<PagedResult<UserViewModel>> GetUsersPaging(GetUserPagingRequest request)
        {
            var query = _userManage.Users;
            if (!String.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.UserName.Contains(request.Keyword) || x.Email.Contains(request.Keyword));
            }

            int totalRow = await query.CountAsync();
            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                            .Take(request.PageSize)
                            .Select(x => new UserViewModel()
                            {
                                Id = x.Id,
                                FirstName = x.FirstName,

                                LastName = x.LastName,

                                PhoneNumber = x.PhoneNumber,

                                UserName = x.UserName,

                                Email = x.Email
                            }).ToListAsync();
            var pageResult = new PagedResult<UserViewModel>()
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pageResult;
        }
    }
}