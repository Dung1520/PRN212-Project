using BusinessObjects;
using Microsoft.Extensions.Configuration;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services
{
    public interface IAuthService
    {
        LoginUser? Login(string usernameOrEmail, string password);
    }
}