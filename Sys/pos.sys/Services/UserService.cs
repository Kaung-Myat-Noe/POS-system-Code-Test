using pos.sys.Entities;
using pos.sys.Models;
using pos.sys.Repositories;
using AspNetCore.ServiceRegistration.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace pos.sys.Services
{
    [ScopedService]
    public interface IUserService
    {
        IList<UserModel> GetUser();
        UserModel Login(LoginModel loginModel);
    }
    public class UserService : IUserService
    {
        internal readonly IUserRepository _user;
        internal ILogger<UserService> _logger;
        internal readonly ApplicationDBContext _ctx;
        internal readonly IMapper _mapper;

        public UserService(IUserRepository user, ILogger<UserService> logger, IMapper mapper, ApplicationDBContext ctx)
        {
            _user = user;
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public IList<UserModel> GetUser()
        {
            // return _user.GetAll().ToList();
            return _mapper.Map<List<UserModel>>(_user.GetAll().ToList());
        }

        public UserModel Login(LoginModel loginModel)
        {
            UserModel userModel = new();
            try
            {
                userModel = _mapper.Map<UserModel>(_user.Query(x => x.email.Equals(loginModel.email) && x.password.Equals(loginModel.password)).FirstOrDefault());
            }
            catch (Exception ex)
            {
                userModel = null;
                _logger.LogError(ex.ToString());
            }
            return userModel;
        }
    }
}